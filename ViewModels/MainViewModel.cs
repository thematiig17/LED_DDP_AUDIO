using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LED_DDP_DRIVER.Models;
using LED_DDP_DRIVER.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace LED_DDP_DRIVER.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly FileIOService _fileService;
        private UDPService _udpService;
        private DDPEngine _ddpEngine;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullAddress))]
        private string _ipAddress;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullAddress))]
        private int _port;
        private CancellationTokenSource _settingsDebounceTokenSource;
        public string FullAddress => $"Current settings: {IpAddress}:{Port}";

        [ObservableProperty] private string _infoLogs = string.Empty;
        [ObservableProperty] private string _ddpLogs = string.Empty;

        public event Action OnInfoLogAdded;
        public event Action OnDdpLogAdded;

        // Audio
        public List<IAudioMode> AvailableModes { get; set; }
        public AudioConfig AudioSettings { get; private set; } = new AudioConfig();
        private CancellationTokenSource _saveDebounceTokenSource;
        [ObservableProperty]
        private IAudioMode _selectedMode;

        // Data for current color display
        [ObservableProperty] private byte _currentR;
        [ObservableProperty] private byte _currentG;
        [ObservableProperty] private byte _currentB;
        [ObservableProperty]
        private SolidColorBrush _masterColorBrush = new SolidColorBrush(Colors.Black);

        public MainViewModel()
        {
            _fileService = new FileIOService();
            LoadApplicationSettings();
            WeakReferenceMessenger.Default.Register<LogMessage>(this, (recipient, message) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (message.Type == LogType.Info || message.Type == LogType.Warn || message.Type == LogType.Error)
                    {
                        string newInfo = InfoLogs + message.Message + Environment.NewLine;
                        if (newInfo.Length > 1000) newInfo = newInfo.Substring(newInfo.Length - 1000);

                        InfoLogs = newInfo;
                        OnInfoLogAdded?.Invoke();
                    }
                    else if (message.Type == LogType.DDP || message.Type == LogType.UDP)
                    {
                        string newDdp = DdpLogs + message.Message + Environment.NewLine;
                        if (newDdp.Length > 1000) newDdp = newDdp.Substring(newDdp.Length - 1000);

                        DdpLogs = newDdp;
                        OnDdpLogAdded?.Invoke();
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<DdpColorMessage>(this, (recipient, message) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CurrentR = message.R;
                    CurrentG = message.G;
                    CurrentB = message.B;
                    MasterColorBrush = new SolidColorBrush(Color.FromRgb(message.R, message.G, message.B));
                });
            });
            //SimulateLogs();
        }

        private void LoadApplicationSettings()
        {
            var config = _fileService.LoadSettings();
            IpAddress = config.IpAddress;
            Port = config.Port;
            AudioSettings = _fileService.LoadAudioSettings();
            AudioSettings.PropertyChanged += OnAudioSettingsChanged;
            AvailableModes = AudioModeRegistry.GetAvailableModes();
            if (AvailableModes.Count > 0)
            {
                SelectedMode = AvailableModes[0];
            }
            Logger.Info("Init complete.");
        }
        private async void SimulateLogs()
        {
            Logger.Info("DDP init...");
            await System.Threading.Tasks.Task.Run(async () =>
            {
                int packetId = 1;
                while (true)
                {
                    Logger.Ddp($"DDP Packet: #{packetId++} (R)CH1=255, (G)CH2=128, (B)CH3=0");
                    await System.Threading.Tasks.Task.Delay(200);
                }
            });
        }
        [RelayCommand]
        private void RunDDPService()
        {
            try
            {
                if (_ddpEngine != null)
                {
                    Logger.Warn("DDP Service is already running!");
                    return;
                }

                Logger.Info("DDP and Audio Service Initialization...");

                var audio = new AudioService();
                var udp = new UDPService(IpAddress, Port);

                _ddpEngine = new DDPEngine(audio, udp, AvailableModes[0], AudioSettings);
                _ddpEngine.Start();

                Logger.Info("DDP Service started.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during DDP initialization: {ex.Message}");
                _ddpEngine = null;
            }
        }
        [RelayCommand]
        private void StopDDPService()
        {
            if (_ddpEngine != null)
            {
                _ddpEngine.Stop();
                _ddpEngine = null;
                Logger.Info("DDP Service stopped.");
            }
        }
        private void OnAudioSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            _saveDebounceTokenSource?.Cancel();
            _saveDebounceTokenSource = new CancellationTokenSource();
            var token = _saveDebounceTokenSource.Token;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000, token);
                    _fileService.SaveAudioConfig(AudioSettings);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Logger.Info("Auto saved audio settings.");
                    });
                }
                catch (TaskCanceledException)
                {
                }
            });
        }
        partial void OnIpAddressChanged(string value)
        {
            TriggerSettingsSave();
        }

        partial void OnPortChanged(int value)
        {
            TriggerSettingsSave();
        }
        private void TriggerSettingsSave()
        {
            _settingsDebounceTokenSource?.Cancel();
            _settingsDebounceTokenSource = new CancellationTokenSource();
            var token = _settingsDebounceTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000, token);

                    var configToSave = new AppConfig
                    {
                        IpAddress = this.IpAddress,
                        Port = this.Port
                    };
                    _fileService.SaveSettings(configToSave);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Logger.Info("Auto saved config.");
                    });
                }
                catch (TaskCanceledException)
                {
                }
            });
        }
        partial void OnSelectedModeChanged(IAudioMode value)
        {
            if (_ddpEngine != null && value != null)
            {
                _ddpEngine.ActiveMode = value;
                Logger.Info($"Changed visualization mode to: {value.Name}");
            }
        }
    }
}

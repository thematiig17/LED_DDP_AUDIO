using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LED_DDP_DRIVER.Models;
using LED_DDP_DRIVER.Services;
using System;
using System.Collections.Generic;
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
        public string FullAddress => $"Current settings: {IpAddress}:{Port}";

        [ObservableProperty] private string _infoLogs = string.Empty;
        [ObservableProperty] private string _ddpLogs = string.Empty;

        public event Action OnInfoLogAdded;
        public event Action OnDdpLogAdded;

        // Audio
        public List<IAudioMode> AvailableModes { get; set; }
        public AudioConfig AudioSettings { get; } = new AudioConfig();

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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (message.Type == LogType.Info)
                    {
                        InfoLogs += message.Message + Environment.NewLine;
                        OnInfoLogAdded?.Invoke();
                    }
                    else if (message.Type == LogType.Ddp)
                    {
                        DdpLogs += message.Message + Environment.NewLine;
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
            Logger.Info("Init complete.");
            AvailableModes = AudioModeRegistry.GetAvailableModes();
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
                    Logger.Info("WARN: DDP Service is already running!");
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
                Logger.Info($"ERROR DURING STARTUP: {ex.Message}");
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
    }
}

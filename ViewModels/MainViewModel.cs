using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LED_DDP_DRIVER.Models;
using LED_DDP_DRIVER.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace LED_DDP_DRIVER.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly FileIOService _fileService;
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
            SimulateLogs();
        }

        private void LoadApplicationSettings()
        {
            var config = _fileService.LoadSettings();
            IpAddress = config.IpAddress;
            Port = config.Port;
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
        private void SaveSettings()
        {
            // 

            Logger.Info("Kliknięto przycisk zapisu ustawień!");
        }
    }
}

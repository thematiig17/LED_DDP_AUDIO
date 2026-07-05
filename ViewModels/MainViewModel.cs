using LED_DDP_DRIVER.Services;
using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LED_DDP_DRIVER.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly FileIOService _fileService;
        [ObservableProperty]
        private string _ipAddress;
        [ObservableProperty]
        private int _port;

        public MainViewModel()
        {
            _fileService = new FileIOService();
            LoadApplicationSettings();
        }

        private void LoadApplicationSettings()
        {
            var config = _fileService.LoadSettings();
            IpAddress = config.IpAddress;
            Port = config.Port;
        }
    }
}

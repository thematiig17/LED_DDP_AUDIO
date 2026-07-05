using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace LED_DDP_DRIVER.Models
{
    public partial class AudioConfig : ObservableObject
    {
        // Gains
        [ObservableProperty] private float _bassGain = 5.0f;
        [ObservableProperty] private float _midGain = 3.0f;
        [ObservableProperty] private float _highGain = 4.0f;

        // Thresholds
        [ObservableProperty] private float _bassThreshold = 0.0f;
        [ObservableProperty] private float _midThreshold = 0.0f;
        [ObservableProperty] private float _highThreshold = 0.0f;

        // Decays
        [ObservableProperty] private float _bassDecay = 0.80f;
        [ObservableProperty] private float _midDecay = 0.85f;
        [ObservableProperty] private float _highDecay = 0.90f;

        // Others
        [ObservableProperty] private float _decay = 0.85f;
        [ObservableProperty] private float _masterBrightness = 1.0f;
    }
}

using CommunityToolkit.Mvvm.Messaging;
using LED_DDP_DRIVER.Services;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Text;

namespace LED_DDP_DRIVER.Models
{
    public class DDPEngine
    {
        private readonly AudioService _audioService;
        private readonly UDPService _udpService;
        private readonly AudioConfig _config;
        public IAudioMode ActiveMode { get; set; }
        private float[] _smoothedMagnitudes;

        public DDPEngine(AudioService audioService, UDPService udpService, IAudioMode defaultMode, AudioConfig config)
        {
            _audioService = audioService;
            _udpService = udpService;
            ActiveMode = defaultMode;
            _config = config;
            _smoothedMagnitudes = new float[512];

            _audioService.OnFftCalculated += HandleFftData;
        }

        public void Start()
        {
            _audioService.StartRecording();
            Logger.Info("Started audio analysis for DDP.");
        }

        public void Stop()
        {
            _audioService.StopRecording();
            Logger.Ddp($"[DDP] End Code: R=0. G=0, B=0");
            _udpService.SendDdpPacket(0, 0, 0);
            WeakReferenceMessenger.Default.Send(new DdpColorMessage(0, 0, 0));
            Logger.Info("Stopped audio analysis.");
        }

        private void HandleFftData(object sender, FftEventArgs e)
        {
            if (ActiveMode == null) return;

            float[] processedMagnitudes = new float[e.Magnitudes.Length];

            //Pre-processing
            for (int i = 0; i < e.Magnitudes.Length; i++)
            {
                float hz = i * e.HzPerBin;
                float mag = e.Magnitudes[i];
                float currentDecay = 0f;

                if (hz >= 20 && hz < 300)
                {
                    mag = (_config.IsBassEnabled && mag >= _config.BassThreshold) ? (mag - _config.BassThreshold) * _config.BassGain : 0;
                    currentDecay = _config.BassDecay;
                }
                else if (hz >= 300 && hz < 4000)
                {
                    mag = (_config.IsMidEnabled && mag >= _config.MidThreshold) ? (mag - _config.MidThreshold) * _config.MidGain : 0;
                    currentDecay = _config.MidDecay;
                }
                else if (hz >= 4000 && hz <= 15000)
                {
                    mag = (_config.IsHighEnabled && mag >= _config.HighThreshold) ? (mag - _config.HighThreshold) * _config.HighGain : 0;
                    currentDecay = _config.HighDecay;
                }
                else
                {
                    mag = 0;
                }

                // Decay smoothing
                if (mag > _smoothedMagnitudes[i])
                {
                    _smoothedMagnitudes[i] = mag;
                }
                else
                {
                    _smoothedMagnitudes[i] *= currentDecay;
                }
                processedMagnitudes[i] = _smoothedMagnitudes[i];
            }

            var colors = ActiveMode.CalculateColors(processedMagnitudes, e.HzPerBin);

            byte finalR = (byte)(colors.R * _config.MasterBrightness);
            byte finalG = (byte)(colors.G * _config.MasterBrightness);
            byte finalB = (byte)(colors.B * _config.MasterBrightness);

            Logger.Ddp($"[DDP] Sending: R={finalR}, G={finalG}, B={finalB}");
            _udpService.SendDdpPacket(finalR, finalG, finalB);
            WeakReferenceMessenger.Default.Send(new DdpColorMessage(finalR, finalG, finalB));
        }
    }
}

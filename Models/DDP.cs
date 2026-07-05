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
        public IAudioMode ActiveMode { get; set; }

        public DDPEngine(AudioService audioService, UDPService udpService, IAudioMode defaultMode)
        {
            _audioService = audioService;
            _udpService = udpService;
            ActiveMode = defaultMode;

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
            Logger.Info("Stopped audio analysis.");
        }

        private void HandleFftData(object sender, FftEventArgs e)
        {
            if (ActiveMode == null) return;
            var colors = ActiveMode.CalculateColors(e.Magnitudes, e.HzPerBin);
            Logger.Ddp($"[DDP] Sending: R={colors.R}, G={colors.G}, B={colors.B}");
            _udpService.SendDdpPacket(colors.R, colors.G, colors.B);
        }
    }
}

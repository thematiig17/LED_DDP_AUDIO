using System;
using System.Collections.Generic;
using System.Text;

namespace LED_DDP_DRIVER.Models
{
    public interface IAudioMode
    {
        string Name { get; }
        (byte R, byte G, byte B) CalculateColors(float[] fftMagnitudes, float hzPerBin);
    }

    public class BasicRgbMode : IAudioMode
    {
        public string Name => "Basic RGB Mode";

        private double _currentRed = 0;
        private double _currentGreen = 0;
        private double _currentBlue = 0;

        public (byte R, byte G, byte B) CalculateColors(float[] fftMagnitudes, float hzPerBin)
        {
            double bassEnergy = 0;
            double midEnergy = 0;
            double highEnergy = 0;

            for (int i = 1; i < fftMagnitudes.Length; i++)
            {
                float hz = i * hzPerBin;
                double magnitude = fftMagnitudes[i];

                if (hz >= 20 && hz < 300) bassEnergy += magnitude;
                else if (hz >= 300 && hz < 4000) midEnergy += magnitude;
                else if (hz >= 4000 && hz <= 15000) highEnergy += magnitude;
            }

            //not used in this basic mode because of pre-processing
            double bassThreshold = 0.0;
            double bassGain = 1.0;
            double midThreshold = 0.0;
            double midGain = 1.0;
            double highThreshold = 0.0;
            double highGain = 1.0;

            double targetRed = bassEnergy > bassThreshold ? (bassEnergy - bassThreshold) * bassGain * 255 : 0;
            double targetGreen = midEnergy > midThreshold ? (midEnergy - midThreshold) * midGain * 255 : 0;
            double targetBlue = highEnergy > highThreshold ? (highEnergy - highThreshold) * highGain * 255 : 0;

            targetRed = Math.Min(255, targetRed);
            targetGreen = Math.Min(255, targetGreen);
            targetBlue = Math.Min(255, targetBlue);

            _currentRed = targetRed > _currentRed ? targetRed : _currentRed * 0.85;
            _currentGreen = targetGreen > _currentGreen ? targetGreen : _currentGreen * 0.85;
            _currentBlue = targetBlue > _currentBlue ? targetBlue : _currentBlue * 0.85;

            byte r = (byte)Math.Min(255, Math.Max(0, _currentRed));
            byte g = (byte)Math.Min(255, Math.Max(0, _currentGreen));
            byte b = (byte)Math.Min(255, Math.Max(0, _currentBlue));

            return (r, g, b);
        }
    }

    public static class AudioModeRegistry
    {
        public static List<IAudioMode> GetAvailableModes()
        {
            return new List<IAudioMode>
            {
                new BasicRgbMode(),
            };
        }
    }
}

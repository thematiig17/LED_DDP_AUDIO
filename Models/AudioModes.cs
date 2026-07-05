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

    public class ClassicBassTrebleMode : IAudioMode
    {
        public string Name => "Basic (Lows + Highs)";

        private double _currentRed = 0;
        private double _currentBlue = 0;

        public (byte R, byte G, byte B) CalculateColors(float[] fftMagnitudes, float hzPerBin)
        {
            double bassEnergy = 0;
            double highEnergy = 0;

            for (int i = 1; i < fftMagnitudes.Length; i++)
            {
                float hz = i * hzPerBin;
                double magnitude = fftMagnitudes[i];

                if (hz >= 20 && hz <= 250) bassEnergy += magnitude;
                if (hz >= 4000 && hz <= 15000) highEnergy += magnitude;
            }

            double bassThreshold = 0.0;
            double bassGain = 1.0;
            double highThreshold = 0.0;
            double highGain = 1.0;

            double targetRed = bassEnergy > bassThreshold ? (bassEnergy - bassThreshold) * bassGain * 255 : 0;
            double targetBlue = highEnergy > highThreshold ? (highEnergy - highThreshold) * highGain * 255 : 0;

            targetRed = Math.Min(255, targetRed);
            targetBlue = Math.Min(255, targetBlue);

            _currentRed = targetRed > _currentRed ? targetRed : _currentRed * 0.85;
            _currentBlue = targetBlue > _currentBlue ? targetBlue : _currentBlue * 0.85;

            byte red = (byte)Math.Min(255, Math.Max(0, _currentRed));
            byte blue = (byte)Math.Min(255, Math.Max(0, _currentBlue));
            byte green = 0;

            if (blue > red + 20)
            {
                red = (byte)(blue * 0.5);
                green = 0;
            }

            return (red, green, blue);
        }
    }

    public static class AudioModeRegistry
    {
        public static List<IAudioMode> GetAvailableModes()
        {
            return new List<IAudioMode>
            {
                new ClassicBassTrebleMode(),
            };
        }
    }
}

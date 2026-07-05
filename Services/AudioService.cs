using NAudio.Wave;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Text;

namespace LED_DDP_DRIVER.Services
{
    public class FftEventArgs : EventArgs
    {
        public float[] Magnitudes { get; set; }
        public float HzPerBin { get; set; }
    }

    public class AudioService
    {
        private WasapiLoopbackCapture _capture;
        private readonly int _fftLength = 1024;
        private float[] _sampleBuffer;
        private int _bufferIndex = 0;

        public event EventHandler<FftEventArgs> OnFftCalculated;

        public void StartRecording()
        {
            _sampleBuffer = new float[_fftLength];
            _capture = new WasapiLoopbackCapture();
            _capture.DataAvailable += ProcessAudioData;
            _capture.StartRecording();
        }

        public void StopRecording()
        {
            _capture?.StopRecording();
            _capture?.Dispose();
        }

        private void ProcessAudioData(object sender, WaveInEventArgs e)
        {
            int bytesPerSample = _capture.WaveFormat.BitsPerSample / 8;
            int channels = _capture.WaveFormat.Channels;

            for (int i = 0; i < e.BytesRecorded; i += bytesPerSample * channels)
            {
                float sampleLeft = BitConverter.ToSingle(e.Buffer, i);
                float sampleRight = channels > 1 ? BitConverter.ToSingle(e.Buffer, i + bytesPerSample) : sampleLeft;

                _sampleBuffer[_bufferIndex++] = (sampleLeft + sampleRight) / 2f;

                if (_bufferIndex >= _fftLength)
                {
                    CalculateFft();
                    _bufferIndex = 0;
                }
            }
        }

        private void CalculateFft()
        {
            Complex[] fftBuffer = new Complex[_fftLength];
            for (int i = 0; i < _fftLength; i++)
            {
                double window = FastFourierTransform.HannWindow(i, _fftLength);
                fftBuffer[i].X = (float)(_sampleBuffer[i] * window);
                fftBuffer[i].Y = 0;
            }

            FastFourierTransform.FFT(true, (int)Math.Log(_fftLength, 2), fftBuffer);

            float[] magnitudes = new float[_fftLength / 2];
            for (int i = 1; i < _fftLength / 2; i++)
            {
                magnitudes[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
            }

            float hzPerBin = (float)_capture.WaveFormat.SampleRate / _fftLength;

            OnFftCalculated?.Invoke(this, new FftEventArgs
            {
                Magnitudes = magnitudes,
                HzPerBin = hzPerBin
            });
        }
    }
}

using NAudio.Wave;
using NVorbis;

namespace CMDG
{
    public class VorbisWaveProvider : IWaveProvider
    {
        private VorbisReader _reader;
        private WaveFormat _waveFormat;
        private float[] _floatBuffer;

        public VorbisWaveProvider(VorbisReader reader)
        {
            _reader = reader;
            _waveFormat = new WaveFormat(_reader.SampleRate, 16, _reader.Channels);
            _floatBuffer = new float[_reader.SampleRate * _reader.Channels]; // Buffer size
        }

        public WaveFormat WaveFormat => _waveFormat;

        public int Read(byte[] buffer, int offset, int count)
        {
            int samplesNeeded = count / 2; // 16-bit samples (2 bytes per sample)
            int samplesRead = _reader.ReadSamples(_floatBuffer, 0, samplesNeeded);

            if (samplesRead == 0)
            {
                return 0; // End of stream
            }

            // Convert float samples to PCM 16-bit
            for (int i = 0; i < samplesRead; i++)
            {
                short sample = (short)(_floatBuffer[i] * short.MaxValue);
                buffer[offset + i * 2] = (byte)(sample & 0xFF);
                buffer[offset + i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
            }

            return samplesRead * 2; // Return bytes read
        }
    }
} 
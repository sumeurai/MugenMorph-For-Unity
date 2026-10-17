using System;
using System.IO;
using UnityEngine;

namespace Mugen3D
{
    public static class NAudioPlayer
    {
        static Func<byte[], AudioClip> mp3Decoder;

        public static void RegisterMp3Decoder(Func<byte[], AudioClip> decoder)
        {
            mp3Decoder = decoder;
        }

        /// <summary>
        /// Decode audio bytes into an AudioClip. WAV on every platform. MP3 on Windows.
        /// </summary>
        public static AudioClip FromAudioData(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 4)
            {
                throw new ArgumentException("Audio buffer is empty or too short.");
            }

            if (IsWav(buffer))
            {
                return DecodeWav(buffer);
            }

            if (mp3Decoder != null)
            {
                return mp3Decoder(buffer);
            }

            throw new NotSupportedException(
                "This clip is not WAV. MP3 playback is available on Windows only.");
        }

        /// <summary>
        /// Backward-compatible entry. Auto-detects WAV / MP3.
        /// </summary>
        public static AudioClip FromWavData(byte[] buffer)
        {
            return FromAudioData(buffer);
        }

        static bool IsWav(byte[] buffer)
        {
            return buffer.Length >= 12
                   && buffer[0] == (byte)'R'
                   && buffer[1] == (byte)'I'
                   && buffer[2] == (byte)'F'
                   && buffer[3] == (byte)'F'
                   && buffer[8] == (byte)'W'
                   && buffer[9] == (byte)'A'
                   && buffer[10] == (byte)'V'
                   && buffer[11] == (byte)'E';
        }

        static AudioClip DecodeWav(byte[] buffer)
        {
            int channels;
            int sampleRate;
            int bitsPerSample;
            int audioFormat;
            byte[] data = ReadWavData(buffer, out channels, out sampleRate, out bitsPerSample, out audioFormat);
            int bytesPerSample = bitsPerSample / 8;
            if (channels <= 0 || bytesPerSample <= 0)
            {
                throw new FormatException("WAV header is missing channel or sample size.");
            }

            int blockAlign = bytesPerSample * channels;
            int frameCount = data.Length / blockAlign;
            if (frameCount <= 0)
            {
                throw new FormatException("Decoded audio has no samples.");
            }

            float[] samples = new float[frameCount];
            if (audioFormat == 1 && bitsPerSample == 16)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    int pos = i * blockAlign;
                    short s = (short)(data[pos] | (data[pos + 1] << 8));
                    samples[i] = s / 32768.0f;
                }
            }
            else if (audioFormat == 3 && bitsPerSample == 32)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    samples[i] = BitConverter.ToSingle(data, i * blockAlign);
                }
            }
            else
            {
                throw new NotSupportedException(
                    "Only 16-bit PCM and 32-bit float WAV are supported on this platform.");
            }

            AudioClip audioClip = AudioClip.Create("audioClip", frameCount, 1, sampleRate, false);
            audioClip.SetData(samples, 0);
            return audioClip;
        }

        static byte[] ReadWavData(
            byte[] buffer,
            out int channels,
            out int sampleRate,
            out int bitsPerSample,
            out int audioFormat)
        {
            channels = 0;
            sampleRate = 0;
            bitsPerSample = 0;
            audioFormat = 0;
            byte[] data = null;

            int offset = 12;
            while (offset + 8 <= buffer.Length)
            {
                string id = EncodingAscii(buffer, offset, 4);
                int size = BitConverter.ToInt32(buffer, offset + 4);
                int payload = offset + 8;
                if (size < 0 || payload + size > buffer.Length)
                {
                    break;
                }

                if (id == "fmt " && size >= 16)
                {
                    audioFormat = BitConverter.ToInt16(buffer, payload);
                    channels = BitConverter.ToInt16(buffer, payload + 2);
                    sampleRate = BitConverter.ToInt32(buffer, payload + 4);
                    bitsPerSample = BitConverter.ToInt16(buffer, payload + 14);
                }
                else if (id == "data")
                {
                    data = new byte[size];
                    Buffer.BlockCopy(buffer, payload, data, 0, size);
                }

                offset = payload + size + (size & 1);
            }

            if (data == null)
            {
                throw new FormatException("WAV file has no data chunk.");
            }

            return data;
        }

        static string EncodingAscii(byte[] buffer, int offset, int count)
        {
            char[] chars = new char[count];
            for (int i = 0; i < count; i++)
            {
                chars[i] = (char)buffer[offset + i];
            }

            return new string(chars);
        }
    }
}

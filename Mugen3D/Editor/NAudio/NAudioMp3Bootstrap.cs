#if UNITY_EDITOR_WIN
using System;
using System.IO;
using NAudio.Wave;
using UnityEngine;
using Mugen3D;

namespace Mugen3D.Editor
{
    static class NAudioMp3Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            NAudioPlayer.RegisterMp3Decoder(Decode);
        }

        static AudioClip Decode(byte[] buffer)
        {
            using (MemoryStream input = new MemoryStream(buffer))
            using (WaveStream reader = new Mp3FileReader(input))
            {
                return ReadPcm16(reader);
            }
        }

        static AudioClip ReadPcm16(WaveStream reader)
        {
            WaveStream pcmStream = null;
            WaveStream source = reader;
            try
            {
                if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm ||
                    reader.WaveFormat.BitsPerSample != 16)
                {
                    pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
                    source = pcmStream;
                }

                byte[] pcmBytes = ReadAllBytes(source);
                return CreateClipFromPcm16(pcmBytes, source.WaveFormat);
            }
            finally
            {
                if (pcmStream != null)
                {
                    pcmStream.Dispose();
                }
            }
        }

        static byte[] ReadAllBytes(WaveStream source)
        {
            source.Position = 0;
            if (source.Length > 0)
            {
                byte[] bytes = new byte[source.Length];
                int offset = 0;
                int read;
                while (offset < bytes.Length &&
                       (read = source.Read(bytes, offset, bytes.Length - offset)) > 0)
                {
                    offset += read;
                }

                if (offset == bytes.Length)
                {
                    return bytes;
                }

                byte[] trimmed = new byte[offset];
                Buffer.BlockCopy(bytes, 0, trimmed, 0, offset);
                return trimmed;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] chunk = new byte[4096];
                int read;
                while ((read = source.Read(chunk, 0, chunk.Length)) > 0)
                {
                    ms.Write(chunk, 0, read);
                }

                return ms.ToArray();
            }
        }

        static AudioClip CreateClipFromPcm16(byte[] pcmBytes, WaveFormat format)
        {
            int channels = Math.Max(1, format.Channels);
            int sampleRate = format.SampleRate;
            int bytesPerSample = Math.Max(1, format.BitsPerSample / 8);
            int frameCount = pcmBytes.Length / (bytesPerSample * channels);
            if (frameCount <= 0)
            {
                throw new FormatException("Decoded audio has no samples.");
            }

            float[] samples = new float[frameCount];
            int step = bytesPerSample * channels;
            for (int i = 0; i < frameCount; i++)
            {
                int pos = i * step;
                short s = (short)(pcmBytes[pos] | (pcmBytes[pos + 1] << 8));
                samples[i] = s / 32768.0f;
            }

            AudioClip audioClip = AudioClip.Create("audioClip", frameCount, 1, sampleRate, false);
            audioClip.SetData(samples, 0);
            return audioClip;
        }
    }
}
#endif

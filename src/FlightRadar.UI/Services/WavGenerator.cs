using System;

namespace FlightRadar.UI.Services;

public static class WavGenerator
{
    public static byte[] GeneratePing(int frequency = 800, float duration = 0.25f, int sampleRate = 44100, float amplitude = 0.5f)
    {
        var numSamples = (int)(sampleRate * duration);
        var dataSize = numSamples * 2;
        var wav = new byte[44 + dataSize];

        WriteAscii(wav, 0, "RIFF");
        WriteInt32(wav, 4, 36 + dataSize);
        WriteAscii(wav, 8, "WAVE");
        WriteAscii(wav, 12, "fmt ");
        WriteInt32(wav, 16, 16);
        WriteInt16(wav, 20, 1);
        WriteInt16(wav, 22, 1);
        WriteInt32(wav, 24, sampleRate);
        WriteInt32(wav, 28, sampleRate * 2);
        WriteInt16(wav, 32, 2);
        WriteInt16(wav, 34, 16);
        WriteAscii(wav, 36, "data");
        WriteInt32(wav, 40, dataSize);

        for (int i = 0; i < numSamples; i++)
        {
            double t = i / (double)sampleRate;
            double envelope = amplitude * Math.Pow(0.001 / amplitude, t / duration);
            double sample = envelope * short.MaxValue * Math.Sin(2 * Math.PI * frequency * t);
            WriteInt16(wav, 44 + i * 2, (short)Math.Clamp(sample, short.MinValue, short.MaxValue));
        }

        return wav;
    }

    private static void WriteAscii(byte[] buffer, int offset, string value)
    {
        for (int i = 0; i < value.Length; i++)
            buffer[offset + i] = (byte)value[i];
    }

    private static void WriteInt16(byte[] buffer, int offset, short value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}

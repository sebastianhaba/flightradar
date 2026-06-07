using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using FlightRadar.UI.Services;

namespace FlightRadar.UI.Desktop;

public class DesktopPingPlayer : IPingPlayer
{
    private static readonly string TempFile = Path.Combine(Path.GetTempPath(), "flightradar-ping.wav");

    public void Play(byte[] wavData)
    {
        try
        {
            File.WriteAllBytes(TempFile, wavData);
            if (PlaySound(TempFile, IntPtr.Zero, SND_FILENAME | SND_ASYNC | SND_NODEFAULT))
                return;
        }
        catch
        {
        }

#pragma warning disable CA1416
        try { Console.Beep(1000, 200); } catch { }
#pragma warning restore CA1416
    }

    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_FILENAME = 0x00020000;
    private const uint SND_ASYNC = 0x0001;
    private const uint SND_NODEFAULT = 0x0002;
}

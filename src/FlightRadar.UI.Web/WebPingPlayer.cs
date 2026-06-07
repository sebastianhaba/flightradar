using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using FlightRadar.UI.Services;

namespace FlightRadar.UI.Web;

[SupportedOSPlatform("browser")]
public partial class WebPingPlayer : IPingPlayer
{
    public void Play(byte[] wavData)
    {
        var base64 = Convert.ToBase64String(wavData);
        Browser.PlayPingBase64(base64);
    }

    [SupportedOSPlatform("browser")]
    public static partial class Browser
    {
        [JSImport("playPing", "MyInterop")]
        public static partial void PlayPingBase64(string base64);

        [JSImport("initAudio", "MyInterop")]
        public static partial void InitAudio();
    }
}

using System;
using System.Threading.Tasks;
using FlightRadar.Shared;

namespace FlightRadar.UI.Services;

public class PingService
{
    private readonly IPingPlayer _player;
    private DateTime _lastPing = DateTime.MinValue;
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(250);

    public PingService(IPingPlayer player)
    {
        _player = player;
    }

    public static Action<bool>? OnMuteChanged { get; set; }
    public static Action? RequestAudioInit { get; set; }

    public bool IsMuted { get; set; } = true;

    public void RequestPing()
    {
        if (IsMuted) return;

        var now = DateTime.UtcNow;
        if (now - _lastPing < DebounceWindow)
            return;

        _lastPing = now;
        _ = PlayAsync();
    }

    private async Task PlayAsync()
    {
        await Task.Yield();
        var wav = WavGenerator.GeneratePing();
        _player.Play(wav);
    }
}

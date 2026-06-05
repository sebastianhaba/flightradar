using FlightRadar.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace FlightRadar.Server.Hubs;

public class RadarHub : Hub
{
    private readonly AdsbPoller _poller;

    public RadarHub(AdsbPoller poller)
    {
        _poller = poller;
    }

    public override async Task OnConnectedAsync()
    {
        var state = _poller.LatestState;
        if (state is not null)
            await Clients.Caller.SendAsync("RadarUpdate", state);

        await base.OnConnectedAsync();
    }
}

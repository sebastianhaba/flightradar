using Microsoft.AspNetCore.SignalR;

namespace FlightRadar.Server.Hubs;

public class RadarHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}

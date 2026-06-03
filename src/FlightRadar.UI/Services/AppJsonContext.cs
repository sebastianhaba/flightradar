using System.Text.Json.Serialization;
using FlightRadar.Shared;

namespace FlightRadar.UI.Services;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RadarState))]
[JsonSerializable(typeof(AircraftData))]
public partial class AppJsonContext : JsonSerializerContext
{
}

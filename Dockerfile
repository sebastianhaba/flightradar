FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ .

RUN apt-get update && apt-get install -y python3 && \
    dotnet workload install wasm-tools
RUN dotnet restore FlightRadar.UI.Web/FlightRadar.UI.Web.csproj
RUN dotnet publish FlightRadar.UI.Web/FlightRadar.UI.Web.csproj -c Release -o /wasm --no-restore

RUN mkdir -p FlightRadar.Server/wwwroot && \
    cp -r /wasm/wwwroot/* FlightRadar.Server/wwwroot/ || \
    cp -r /src/FlightRadar.UI.Web/bin/Release/net10.0/browser-wasm/AppBundle/* FlightRadar.Server/wwwroot/

RUN dotnet restore FlightRadar.Server/FlightRadar.Server.csproj
RUN dotnet publish FlightRadar.Server/FlightRadar.Server.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV RADAR_LAT=52.2297
ENV RADAR_LON=21.0122
ENV POLL_INTERVAL_SECONDS=5
ENV RADAR_RANGE_KM=25
ENV ADSB_API_BASE_URL=https://opendata.adsb.fi/api/v3
ENTRYPOINT ["dotnet", "FlightRadar.Server.dll"]

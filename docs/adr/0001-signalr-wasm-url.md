# 0001-signalr-wasm-url

**Status:** accepted

Klient używa **SignalR** jako jedynego kanału danych.

Problem: WASM nie zna originu strony (relatywny URL `/hubs/radar` błędnie resolwowany jako `file://`), a hardcodowany `http://localhost:8080` działa tylko lokalnie.

Rozwiązanie: przy starcie aplikacji WASM, kod w `Program.cs` odpytuje JavaScript o `window.location.origin` przez `[JSImport]`, zapisuje wynik do `AppOptions.BaseUrl`, a `RadarHubClient` konstruuje z tego pełny URL huba.

Kluczowy pattern JS interop w WASM (do zapamiętania):

1. **Osobny plik JS** (np. `interop.js`) z eksportowanymi funkcjami — nie `main.js` (który odpala `dotnet.create()` i powoduje cykliczną zależność)
2. `JSHost.ImportAsync("Nazwa", "../plik.js")` — ścieżka względem `_framework/`, więc `../` trafia do roota AppBundle
3. `[JSImport("funkcja", "Nazwa")]` na `static partial` metodzie w `Program.cs`
4. `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` w csproj projektu WASM

```csharp
// Program.cs (WASM head)
await JSHost.ImportAsync("MyInterop", "../interop.js");
AppOptions.BaseUrl = Browser.GetOrigin();

[SupportedOSPlatform("browser")]
public static partial class Browser
{
    [JSImport("getOrigin", "MyInterop")]
    public static partial string GetOrigin();
}
```

```javascript
// interop.js
export function getOrigin() {
    return window.location.origin;
}
```

Desktop używa fallbacku: `HUB_URL` env var lub `http://localhost:8080/hubs/radar`.

**Rozważane i odrzucone:**
- HTTP polling `/api/radar` — prostszy w WASM, ale traci real-time push
- Query string `?hub=...` + `.withApplicationArgumentsFromQuery()` — nie przekazuje argów do `Program.Main` w tej wersji runtime
- `create({ environmentVariables })` — nie działa
- `[JSImport]` z `main.js` — cykliczna zależność (main.js odpala `dotnet.create()`)
- Endpoint `/api/hub-url` + `HttpClient.GetStringAsync` — WASM nie radzi sobie z relatywnym URL w `GetStringAsync`

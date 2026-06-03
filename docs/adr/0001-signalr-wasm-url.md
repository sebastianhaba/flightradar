# 0001-signalr-wasm-url

**Status:** accepted

Klient używa **SignalR** jako jedynego kanału danych (nie HTTP polling). Problemem jest rozpoznawanie URLa huba w WASM: relatywne ścieżki (`/hubs/radar`) w runtime WASM błędnie rozpoznawane są jako `file://`, a hardcodowany `http://localhost:8080` działa tylko lokalnie.

Rozwiązanie: `main.js` przed uruchomieniem runtime .NET wstrzykuje origin strony jako query parameter (`?hub=http://origin/hubs/radar`). `.withApplicationArgumentsFromQuery()` przekazuje go jako `--hub=` argument do `Program.Main()`, który ustawia `HUB_URL` env var przed uruchomieniem Avalonii. `RadarHubClient` czyta tę zmienną.

Ten mechanizm omija potrzebę `[JSImport]`/`JSHost.ImportAsync` (które wymagają unsafe code i mają problem z cyklicznymi zależnościami z `main.js`), oraz `withEnvironmentVariable` w bootstrapperze (niedostępne lub niedziałające w tej wersji runtime).

**Desktop:** używa fallbacku `http://localhost:8080/hubs/radar` lub zmiennej `HUB_URL` ustawionej ręcznie.

**Rozważane alternatywy:**
- HTTP polling `/api/radar` — prostszy w WASM, ale traci real-time push i status połączenia
- `[JSImport]` z `JSHost.ImportAsync` — cykliczna zależność z `main.js` (który odpala `dotnet.create()`)
- `create({ environmentVariables })` — nie działa w tej wersji WASM bootstrappera

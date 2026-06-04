# 0002-no-trimming-wasm-release

**Status:** accepted

W buildzie Release dla `browser-wasm`, .NET SDK domyślnie włącza trimming (`PublishTrimmed`) i AOT (`RunAOTCompilation`). Trimming usuwa kod CommunityToolkit.Mvvm oraz bindings Avalonii, przez co **UI przestaje reagować na zmiany danych** (Canvas nie odświeża się, PropertyChanged nie propaguje). Aplikacja działa w tle (SignalR odbiera dane, logi pokazują Received N aircraft), ale interfejs jest statyczny.

W Debug trimming jest domyślnie wyłączony, dlatego lokalnie wszystko działało.

Rozwiązanie: jawne wyłączenie w `.csproj` projektu WASM:

```xml
<PublishTrimmed>false</PublishTrimmed>
<RunAOTCompilation>false</RunAOTCompilation>
```

Koszt: większy binarny WASM (~25 MB vs ~15 MB). Akceptowalny dla aplikacji self-hosted.

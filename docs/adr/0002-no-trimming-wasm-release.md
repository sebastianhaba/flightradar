# 0002-no-trimming-wasm-release

**Status:** accepted

W buildzie Release dla `browser-wasm`, .NET SDK domyślnie włącza trimming (`PublishTrimmed`) i AOT (`RunAOTCompilation`). Trimming usuwa kod CommunityToolkit.Mvvm oraz bindings Avalonii, przez co **UI przestaje reagować na zmiany danych** (Canvas nie odświeża się, PropertyChanged nie propaguje). Aplikacja działa w tle (SignalR odbiera dane, logi pokazują Received N aircraft), ale interfejs jest statyczny.

W Debug trimming jest domyślnie wyłączony, dlatego lokalnie wszystko działało.

Próba częściowego trimowania (`linker.xml` z `preserve="all"` dla Avalonia.* i CommunityToolkit.Mvvm) nie powiodła się — Avalonia ładuje assembly dynamicznie (np. `Avalonia.Controls.ColorPicker`), a trimmer nie widzi tych zależności i usuwa je mimo preserve.

**AOT wymaga włączonego trimowania** (`RunAOTCompilation=true` działa tylko z `PublishTrimmed=true`). Nie można mieć AOT bez trimowania.

Rozwiązanie: oba wyłączone:

```xml
<PublishTrimmed>false</PublishTrimmed>
<RunAOTCompilation>false</RunAOTCompilation>
```

Koszt: większy binarny WASM (~25 MB vs ~15 MB) i wolniejszy start. Akceptowalny dla aplikacji self-hosted.

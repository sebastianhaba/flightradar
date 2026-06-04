# 0003-side-panel

**Status:** accepted

Panel boczny z listą aircraftów i sekcją szczegółów, rozwijany inline (współdzieli przestrzeń z radarem, nie nachodzi).

## Decyzje

### 1. SplitView CompactInline zamiast overlay

`DisplayMode="CompactInline"` — panel w stanie zwiniętym ma 48px (pokazuje ikony), po rozwinięciu 260px. Content (radar) jest wypychany, nie zakrywany.

**Odrzucone:** `DisplayMode="Overlay"` (panel nachodzi na radar — utrudnia interakcję), `DisplayMode="Inline"` (panel znika całkowicie przy zwinięciu — nie widać ikon).

### 2. ListBox z ItemTemplate zamiast DataGrid

DataGrid w Avalonia (v11.3.13) miał problemy z bindingiem do ObservableCollection przez zagnieżdżone ścieżki właściwości i nie przetwarzał CollectionChanged gdy kontrolka była poza drzewem wizualnym. Rozwiązanie zastępcze: `ListBox` z ręcznym szablonem wiersza (`Grid` z kolumnami).

**Koszty:** brak wbudowanego sortowania kolumnami (do dodania później), ręczne nagłówki kolumn, ręczny converter opacity dla stale.

**Odrzucone:** DataGrid z osobnych NuGet (Avalonia.Controls.DataGrid) — wersja 11.3.13 nie pasowała do Avalonia 11.3.17, wersja 12.0.0 wymagała avalonia 12.x.

### 3. AircraftTracker w backendzie

Backendowy `AircraftTracker` trzyma stan aircraftów między pollami (`Dictionary<string, AircraftData>`):
- Ustawia `FirstSeen` przy pierwszym pojawieniu
- Aktualizuje `LastSeen` przy każdym pollu
- Oznacza `IsStale = true` po 30s braku aktualizacji
- Usuwa z listy przy następnym pollu po oznaczeniu jako stale

**Dlaczego nie frontend:** backendowe śledzenie działa niezależnie od reconnectów SignalR i będzie potrzebne do przyszłych ficzerów (historia, statystyki).

### 4. ObservableCollection + Dispatcher.UIThread.Post

Dane przychodzą przez SignalR na background threadzie. Używamy `Dispatcher.UIThread.Post()` do marshalowania na UI thread, a `ObservableCollection` (mutowana in-place przez `Clear()`/`Add()`) + subskrypcja `CollectionChanged` w `RadarCanvas` do odświeżania rysunku.

**Dlaczego nie podmiana całej listy:** ListBox potrzebuje ObservableCollection do śledzenia zmian. Podmiana referencji listy kasowałaby selekcję i wymagała ponownego bindingu.

### 5. Proste przekazywanie referencji między VM

`MainViewModel` przekazuje tę samą instancję `ObservableCollection<AircraftData>` do `SidePanelViewModel` przez konstruktor/property. Brak DI kontenera, brak event busa — proste referencje.

**Wystarczające przy 4 VM.** Refaktor do DI/event bus dopiero gdy liczba VM lub złożoność komunikacji znacząco wzrośnie.

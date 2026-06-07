# FlightRadar

Webowa aplikacja radaru lotniczego pokazująca na żywo statki powietrzne w okolicy punktu domowego. Okrągły radar z koncentrycznymi pierścieniami co 5 km, maksymalny zasięg 25 km. Dane pobierane z publicznego API ADS-B (adsb.fi).

## Language

**Radar**:
Okrągły widok z koncentrycznymi pierścieniami odległości i oznaczeniami kierunków świata, na którym wyświetlane są pozycje statków powietrznych.
_Avoid_: radar screen, scope

**Aircraft**:
Pojedynczy statek powietrzny wykryty w zasięgu radaru. Reprezentowany przez ikonę obróconą zgodnie z headingiem, z callsignem i altitude.
_Avoid_: plane, flight, target (chyba że w kontekście ESP32)

**Home Point**:
Środek radaru — współrzędne geograficzne (lat/lon) ustawiane przez użytkownika jako punkt odniesienia.
_Avoid_: origin, center, station

**Ring**:
Jeden z koncentrycznych okręgów na radarze oznaczających odległość od Home Point. Standardowo co 5 km, do 25 km.
_Avoid_: circle, band, range marker

**Heading**:
Kąt w stopniach (0-360) wskazujący kierunek lotu aircrafta. 0 = północ, 90 = wschód. Ikona aircrafta jest obracana o ten kąt.
_Avoid_: bearing, course, track

**Callsign**:
Identyfikator lotu (np. "LOT123", "BAW456"). Wyświetlany pod ikoną aircrafta na radarze.
_Avoid_: flight number, identifier, registration

**Category**:
Klasyfikacja ADS-B (A0-A7) określająca typ statku powietrznego. A7 = helicopter, pozostałe traktowane jako samolot.
_Avoid_: type, aircraft type

**Stale**:
Aircraft który nie był widziany przez API przez ponad 30 sekund. Wyświetlany w Aircraft Table z opacity 0.5, usuwany z listy w następnym Pollu.
_Avoid_: ghost, expired, timeout

**Poll**:
Cykliczne zapytanie backendu do ADS-B API o listę aircraftów w zasięgu.
_Avoid_: fetch, sync, refresh

**Aircraft Table**:
Tekstowa lista wszystkich aircraftów aktualnie na radarze, wyświetlana w panelu bocznym. Każdy wiersz pokazuje timestamp pierwszego pojawienia, Callsign i podstawowe dane (altitude, heading). Kliknięcie wiersza rozwija szczegóły w dolnej sekcji panelu. Stale aircrafty (niewidziane >30s) wyświetlane są z opacity 0.5.
_Avoid_: log, event log, history

**Side Panel**:
Składany panel boczny po prawej stronie radaru, zawierający Aircraft Table (góra) i sekcję szczegółów (dół). W trybie zwiniętym pokazuje tylko ikony; w trybie rozwiniętym współdzieli przestrzeń z radarem (inline, nie overlay).
_Avoid_: sidebar, drawer, dock

**History**:
Tryb aplikacji umożliwiający przeglądanie aircraftów które pojawiły się na radarze w przeszłości. Przełączany przez przyciski na dole ekranu.
_Avoid_: replay, log, archive

**Live**:
Tryb aplikacji pokazujący dane na żywo z ADS-B API. Domyślny tryb przy uruchomieniu.
_Avoid_: realtime, current, online

**Flight Record**:
Zapisany w bazie LiteDB ciągły pobyt aircrafta na radarze — od pierwszego pojawienia się w Pollu aż do zniknięcia. Zawiera dane metadata (Callsign, Category, Registration) oraz tablicę Track Points.
_Avoid_: session, entry, document

**Track Point**:
Pojedyncza próbka pozycji aircrafta w danym momencie, zawierająca timestamp, pozycję (lat/lon), Heading, Altitude i GroundSpeed. Zbierana przy każdym Pollu.
_Avoid_: sample, dot, position

**Ping**:
Krótki dźwięk (sinus 1000Hz, 0.25s) generowany przez C# jako WAV i odtwarzany platformowo. Pojawia się gdy Sweep przechodzi przez pozycję Aircrafta na Radarze. Jeśli w ciągu 100ms następuje kolejne przejście, jest wyciszane (debouncing).
_Avoid_: beep, blip, alert

**Sweep**:
Obracająca się linia na Radarze symulująca skanowanie radarowe. Przechodzi przez pozycje Aircraftów, które pojawiają się i stopniowo zanikają (fade). Ping jest odtwarzany gdy Sweep osiąga bearing Aircrafta.
_Avoid_: scan line, beam, rotation

**Zone**:
Jedna z czterech 90-stopniowych stref dookoła Home Point, wyznaczonych przez kardynalne kierunki: N (315°–45°), E (45°–135°), S (135°–225°), W (225°–315°). Aircraft w strefie wywołuje Ping gdy przy każdym Pollu w którejś ze stref znajduje się Aircraft.
_Avoid_: quadrant, sector, region

**Mute**:
Stan wyciszenia wszystkich Pingów. Domyślnie włączony (wyciszony), przełączany ikoną na Status Barze. Stan trwały dla WASM (localStorage), nietrwały dla Desktopa.
_Avoid_: silence, audio toggle

**Sweep**:
Obracająca się linia na Radarze symulująca skanowanie radarowe. Przechodzi przez pozycje Aircraftów, które pojawiają się i stopniowo zanikają (fade). Prędkość obrotu i czas zanikania są konfigurowalne.
_Avoid_: scan line, beam, rotation

## Relationships

- **Home Point** jest środkiem **Radaru**
- **Radar** zawiera 5 **Ringów** (5, 10, 15, 20, 25 km)
- **Aircraft** ma pozycję (lat/lon), **Heading**, **Callsign**, altitude i **Category**
- **Category** determinuje ikonę: A7 → helicopter, reszta → airplane
- Backend wykonuje **Poll** co N sekund i pushuje dane przez SignalR do frontendu
- **Side Panel** zawiera **Aircraft Table** (góra) i sekcję szczegółów (dół)
- Kliknięcie wiersza w **Aircraft Table** wypełnia dolną sekcję **Side Panel** szczegółami aircrafta
- Aircraft który nie pojawił się w API przez >30s jest oznaczany jako **stale** i usuwany z listy

## Example dialogue

> **Dev:** "Co się dzieje gdy API ADS-B zwróci 0 aircraftów?"
> **Domain expert:** "Radar pokazuje puste niebo — pierścienie i oznaczenia kierunków są nadal widoczne, ale bez ikon. Status bar pokazuje '0 aircraftów'."
>
> **Dev:** "A gdy aircraft jest poza maksymalnym ringiem (25km) ale w zasięgu fetcha API?"
> **Domain expert:** "Pokazujemy go jako małą kropkę na krawędzi radaru, na odpowiednim bearingu — tak jak robi to ESP32."

## Flagged ambiguities

- "samolot" vs "śmigłowiec" vs "statek powietrzny" — resolved: używamy **Aircraft** jako ogólnego terminu, **Category A7** wskazuje helicopter
- "zasięg" może oznaczać range radaru (25km) lub fetch distance API (~33km) — resolved: **Radar Range** to max pierścień (25km), **Fetch Distance** to większy dystans zapytania do API

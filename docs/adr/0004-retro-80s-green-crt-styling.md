# Retro 80s green-CRT styling

Aplikacja używa stylizacji retro green-CRT inspirowanej filmami z lat 80. (War Games, Firefox) zamiast nowoczesnego ciemnego motywu. Wszystkie elementy UI (tekst, ramki, ikony) używają palety zieleni na czarnym tle, z efektem obracającego się sweepa na radarze.

## Status

Accepted

## Context

Aplikacja potrzebowała spójnej tożsamości wizualnej. Rozważano nowoczesny ciemny motyw (FluentTheme Dark) oraz retro styl green-CRT. Wybrano green-CRT dla charakterystycznego wyglądu nawiązującego do starych radarów lotniczych.

## Decision

- Tło aplikacji: czarne (`#000000`)
- Ramki i obramowania: ciemna zieleń (`#2a5a2a`)
- Tekst główny: jasna zieleń (`#80FF80`)
- Tekst drugorzędny: średnia zieleń (`#4a8a4a`)
- Ikony aircraftów: zielone odcienie (airplane `#00FF80`, helicopter `#00CC60`)
- Przyciski: zielone obramowanie, aktywne wypełnione ciemną zielenią
- Status bar: zielone diody (orange/red dla stanów ostrzegawczych)
- Radar: czarne tło z zielonymi ringami, tickami, etykietami
- Sweep: obracająca się linia z poświatą, efekt fade na aircraftach

## Consequences

- Nadpisanie styli FluentTheme dla DatePicker, Expander, ListBoxItem
- Custom ControlTheme dla ListBoxItem (bez VisualStateManager)
- Wszystkie nowe elementy UI muszą stosować się do green palette
- Ewentualne zmiany stylu wymagają aktualizacji wielu plików AXAML

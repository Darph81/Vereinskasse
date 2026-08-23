# Vereinskasse

Kassensystem für Vereinsfeste zur einfachen Bestellungsaufnahme und Rückgeldberechnung. Die Oberfläche lädt ihre Artikel dynamisch aus einer Excel-Preisliste und protokolliert jede abgeschlossene Bestellung automatisch in einer täglichen Rechnungs-Exceldatei.

## Funktionen

- Vollbild-Kassenoberfläche (Avalonia, plattformübergreifend unter Windows, Linux und macOS)
- Dynamischer Produktbildschirm aus `config/Preisliste.xlsx`, gruppiert nach Kategorie und farblich abgesetzt, Artikel sortiert nach ihrer Position
- Bestellliste mit Mengenanpassung (Löschbutton reduziert die Menge schrittweise, entfernt den Eintrag erst bei 0)
- Touch-optimierter Nummernblock (inkl. Komma-Trenner für Beträge wie `10,50`) zur Eingabe des gegebenen Betrags
- Live-Berechnung des Rückgelds während der Eingabe
- Automatisches Anlegen und Fortschreiben einer täglichen Rechnungsdatei unter `Rechnungen/`

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` sollte `10.0.x` anzeigen)
- Ein unterstütztes Betriebssystem: Windows, Linux oder macOS (Avalonia ist plattformübergreifend)

## Abhängigkeiten (NuGet)

| Paket                     | Version  | Zweck                                                  |
|---------------------------|----------|---------------------------------------------------------|
| Avalonia                  | 12.1.1   | UI-Framework                                            |
| Avalonia.Desktop          | 12.1.1   | Desktop-Backend (Fenster, Eingabe)                      |
| Avalonia.Themes.Fluent    | 12.1.1   | Fluent-Design-Theme                                     |
| Avalonia.Fonts.Inter      | 12.1.1   | Inter-Schriftart                                        |
| Avalonia.Diagnostics      | 11.3.20  | Entwickler-Diagnosewerkzeuge (nur im Debug-Build)        |
| ClosedXML                 | 0.105.1  | Lesen der Preisliste und Schreiben der Rechnungsdateien  |
| CommunityToolkit.Mvvm     | 8.4.2    | MVVM-Hilfsklassen (`ObservableObject`, `RelayCommand`)   |

Alle Pakete werden beim ersten `dotnet restore` bzw. `dotnet build` automatisch von nuget.org geladen — eine manuelle Installation ist nicht nötig.

## Projektstruktur

```
TVM_CalcUI/
├── config/
│   ├── Preisliste.xlsx      # Eingabedatei: Artikel, Preise, Kategorien
│   └── Screen.jpeg          # Layout-Referenz für die Oberfläche
├── Models/                  # Product, CategoryGroup, OrderLine
├── Services/
│   ├── PriceListLoader.cs   # liest config/Preisliste.xlsx ein
│   └── InvoiceLogger.cs     # legt Rechnungen/*.xlsx an und schreibt Zeilen
├── ViewModels/
│   └── MainWindowViewModel.cs
├── MainWindow.axaml(.cs)    # Kassenoberfläche
├── App.axaml(.cs)
├── Program.cs
└── TVM_CalcUI.csproj
```

## Konfiguration

### Preisliste (`config/Preisliste.xlsx`)

Die Artikel-Buttons werden beim Start der Anwendung dynamisch aus dieser Datei erzeugt. Erwartet wird ein Tabellenblatt mit einer Kopfzeile und folgenden Spalten (Reihenfolge der Spalten ist egal, die Zuordnung erfolgt über die Spaltennamen):

| Produkt          | Preis | Kategorie   | Position |
|------------------|-------|-------------|----------|
| Fürstenberg      | 4.00  | Getränke    | 1        |
| Cola, Fanta      | 2.5   | Alkfrei ... | 1        |
| ...              | ...   | ...         | ...      |

- **Produkt** – Name auf dem Button
- **Preis** – Dezimalzahl (Punkt oder Komma als Trennzeichen)
- **Kategorie** – bestimmt Spalte, Gruppierung und Farbe auf dem Bildschirm; die Reihenfolge, in der Kategorien erstmals in der Datei auftauchen, bestimmt die Reihenfolge der Spalten
- **Position** – Sortierung der Artikel innerhalb ihrer Kategorie (aufsteigend)

Die Datei wird beim Build automatisch nach `config/Preisliste.xlsx` neben die ausführbare Datei kopiert. **Jeder Start der Anwendung liest die Datei neu ein** – Änderungen an der Preisliste werden also erst nach einem Neustart wirksam. Fehlt die Datei oder lässt sie sich nicht lesen, erscheint eine Fehlermeldung oberhalb der Produktübersicht.

### Rechnungsprotokoll (`Rechnungen/`)

Beim Start wird — falls noch nicht vorhanden — eine tagesaktuelle Datei

```
Rechnungen/YYYY_MM_DD_Rechnung.xlsx
```

angelegt, mit der Kopfzeile `Gesamt` gefolgt von einer Spalte je Kategorie aus der Preisliste. Jede über **„ABSCHLIESSEN“** abgeschlossene Bestellung wird als neue Zeile hinter die letzte bestehende Zeile angehängt: zuerst der Gesamtpreis, danach je Kategorie die georderte Menge. Die Datei wird nach jeder Bestellung sofort gespeichert. Leere Bestellungen (kein Artikel im Warenkorb) werden nicht protokolliert.

## Ausführen (Entwicklung)

```bash
dotnet restore
dotnet run
```

Die Anwendung startet maximiert/im Vollbild und kann über das Schließen-Symbol (X) oben rechts beendet werden.

## Build

```bash
dotnet build -c Release
```

## Deployment / Veröffentlichen

Für eine eigenständige, verteilbare Anwendung empfiehlt sich `dotnet publish` mit einem passenden Runtime Identifier (RID):

```bash
# Windows x64, self-contained, als Einzeldatei
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
```

Das Ergebnis liegt anschließend unter `bin/Release/net10.0/<RID>/publish/`.

Für einen framework-abhängigen Build (kleinere Ausgabe, benötigt eine installierte .NET 10 Runtime auf dem Zielsystem):

```bash
dotnet publish -c Release -r <RID>
```

**Wichtig beim Deployment:**
- Der Ordner `config/` mit `Preisliste.xlsx` muss neben der ausführbaren Datei liegen (wird durch `dotnet publish` automatisch mit übernommen).
- Der Ordner `Rechnungen/` wird beim ersten Start automatisch neben der Anwendung angelegt — das Zielverzeichnis muss dafür Schreibrechte besitzen.

## Lizenz

Dieses Projekt steht unter der MIT-Lizenz, siehe [LICENSE](LICENSE).

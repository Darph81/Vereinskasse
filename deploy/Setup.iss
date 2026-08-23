; Inno Setup Skript fuer die Vereinskasse
; Erstellt ein Windows-Setup.exe aus dem self-contained win-x64 Publish-Output.
;
; Voraussetzung: Inno Setup 6 (https://jrsoftware.org/isinfo.php)
; Aufruf ueber deploy\build-installer.ps1 (empfohlen) oder direkt:
;   ISCC.exe deploy\Setup.iss
;
; Das Skript erwartet den Publish-Output unter:
;   bin\Release\net10.0\win-x64\publish
; (siehe README.md, Abschnitt "Deployment / Veroeffentlichen")

#define AppName "Vereinskasse"
#define AppVersion "1.0.0"
#define AppPublisher "Darph81"
#define AppURL "https://github.com/Darph81/Vereinskasse"
#define AppExeName "Vereinskasse.exe"
#define PublishDir SourcePath + "..\bin\Release\net10.0\win-x64\publish"

[Setup]
AppId={{F3F66C90-6D4B-4CE6-94FA-39E7DBFC2918}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
; Installation ins Benutzerprofil (kein Admin/UAC noetig) - die Anwendung
; schreibt zur Laufzeit die Rechnungsdatei (Rechnungen\*.xlsx) direkt neben
; die exe und braucht dafuer Schreibrechte im Installationsverzeichnis.
DefaultDirName={localappdata}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename=VereinskasseSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#AppExeName}
DisableWelcomePage=no

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Kompletter Publish-Output (exe, Laufzeit, config\Preisliste.xlsx, ...)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Rechnungsdateien bleiben beim Deinstallieren standardmaessig erhalten (siehe unten).
; Wer sie ebenfalls entfernen moechte, kann diese Zeile aktivieren:
; Type: filesandordirs; Name: "{app}\Rechnungen"

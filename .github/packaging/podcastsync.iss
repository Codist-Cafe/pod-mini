; Inno Setup script for PodcastSync Windows installer.
; Build with:
;   iscc /DMyAppVersion=1.0.0 podcastsync.iss

#define MyAppName "PodcastSync"
#define MyAppPublisher "Codist-Cafe"
#define MyAppURL "https://github.com/Codist-Cafe/pod-mini"
#define MyAppExeName "podcastsync.exe"

#ifndef MyAppVersion
# define MyAppVersion "0.0.0"
#endif

[Setup]
AppId={{2F1A8D3C-5E7B-4A9C-8D1F-3E5A7B9C1D2E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\publish
OutputBaseFilename=PodcastSync-Windows-x64-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ChangesEnvironment=yes

[Files]
Source: "..\publish\win-x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCU; Subkey: "Environment"; ValueType: expandsz; ValueName: "Path"; \
    ValueData: "{olddata};{app}"; Check: NeedsAddPath(ExpandConstant('{app}'))
Root: HKCU; Subkey: "Environment"; ValueType: string; ValueName: "Path"; \
    ValueData: "{olddata}"; Check: not NeedsAddPath(ExpandConstant('{app}'))

[Code]
function NeedsAddPath(Param: string): boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(HKCU, 'Environment', 'Path', OrigPath) then
  begin
    Result := True;
    exit;
  end;
  Result := Pos(';' + Param + ';', ';' + OrigPath + ';') = 0;
end;

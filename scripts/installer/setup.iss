; Inno Setup 6 Script for Once Human: Event Timer
; Author: Murr (https://github.com/vtstv)
; License: MIT License

#ifndef MyAppVersion
  #define MyAppVersion "1.5.3"
#endif

#ifndef SourceDir
  #define SourceDir "..\..\release\OnceHumanTimer-v" + MyAppVersion
#endif

#ifndef OutputDir
  #define OutputDir "..\..\release"
#endif

#ifndef OutputBaseFilename
  #define OutputBaseFilename "OnceHumanTimer-v" + MyAppVersion + "-Setup"
#endif

#define MyAppName "Once Human: Event Timer"
#define MyAppPublisher "Murr"
#define MyAppURL "https://github.com/vtstv/OHTimer"
#define MyAppExeName "OnceHumanEventTimer.exe"

[Setup]
AppId={{9B7E39A2-8214-4B2E-9FD6-C8C56B5D71F4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={localappdata}\Programs\OnceHumanTimer
DisableProgramGroupPage=yes
LicenseFile=..\..\LICENSE
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
SetupIconFile=..\..\src\OnceHumanEventTimer.UI\Assets\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

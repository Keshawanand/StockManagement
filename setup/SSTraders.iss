#define MyAppName "SS Traders Stock Management"
#define MyAppVersion "1.0"
#define MyAppPublisher "SS Traders"
#define MyAppExeName "StockManagementApp.exe"
#define MySourceDir "..\publish\StockManagementApp"

[Setup]
AppId={{B3A7F2E1-4C8D-4F9A-A1B2-C3D4E5F60001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\SS Traders\StockManagement
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=output
OutputBaseFilename=SSTraders_StockManagement_Setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

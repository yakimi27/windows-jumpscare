[Setup]
    AppName=Windows Jumpscare
    AppVersion=1.0.0
    DefaultDirName={autopf}\Windows Jumpscare
    DefaultGroupName=Windows Jumpscare
    OutputDir=dist
    OutputBaseFilename=WindowsJumpscare-Setup
    Compression=lzma2/max
    SolidCompression=yes
    SetupIconFile=Desktop\Assets\Icon\windowsJumpscare.ico
    UninstallDisplayIcon={app}\WindowsJumpscare.exe
    [Tasks]
    Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

    [Files]
    Source: "dist\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

    [Icons]
    Name: "{group}\Windows Jumpscare"; Filename: "{app}\WindowsJumpscare.exe"
    Name: "{commondesktop}\Windows Jumpscare"; Filename: "{app}\WindowsJumpscare.exe"; Tasks: desktopicon

    [Run]
    Filename: "{app}\WindowsJumpscare.exe"; Description: "{cm:LaunchProgram,Windows Jumpscare}"; Flags: nowait postinstall skipifsilent
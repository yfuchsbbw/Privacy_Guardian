# Privacy Guardian Installer

This folder contains the Inno Setup installer definition.

Build steps:

1. Install the .NET 8 SDK.
2. Install Inno Setup 6.
3. Run `PrivacyGuardianApp\Publish-Build.ps1`.

Output:

- Published application: `bin\Release\net8.0-windows\win-x64\publish`
- Installer: `Publish\PrivacyGuardianSetup-1.0.0.exe`

The installer creates Start Menu shortcuts and optionally a Desktop shortcut.

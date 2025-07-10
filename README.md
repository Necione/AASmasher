# AASmasher Mod

A MelonLoader mod for **Operation New Earth** that enhances gameplay with various automation and UI features.

## Features

- **Night Mode Manager**: Enhanced night vision and lighting controls
- **Invasion Manager**: Automated invasion event handling
- **Fat Aliens Manager**: Special alien encounter management
- **Button Automation**: Automated UI interactions
- **Camera Controller**: Advanced camera movement and controls
- **Settings Manager**: Persistent mod configuration
- **GUI System**: Custom ImGui-based interface

## Requirements

### For Building
- **Visual Studio 2017 or later** (or Visual Studio Code with C# extension)
- **.NET Framework 4.8 SDK**
- **NuGet Package Manager**

### For Installation
- **Operation New Earth** (Steam version)
- **MelonLoader** installed in your game directory

## Building the Mod

### 1. Clone the Repository
```bash
git clone https://github.com/Necione/AASmasher.git
cd AASmasher
```

### 2. Restore NuGet Packages
```bash
dotnet restore AASmasher.sln
```

### 3. Update MelonLoader Reference
Before building, you need to update the MelonLoader reference path in `AASmasher.csproj`:

1. Open `AASmasher.csproj` in a text editor
2. Find the MelonLoader reference line:
   ```xml
   <Reference Include="MelonLoader">
     <HintPath>..\..\..\..\..\..\Program Files (x86)\Steam\steamapps\common\Operation New Earth\MelonLoader\net35\MelonLoader.dll</HintPath>
   </Reference>
   ```
3. Update the `<HintPath>` to match your game installation directory

### 4. Build the Project
```bash
dotnet build AASmasher.sln --configuration Release
```

The compiled mod will be in `bin\Release\AASmasher.dll`

## Installing the Mod

### 1. Install MelonLoader
1. Download MelonLoader from the [official releases](https://github.com/LavaGang/MelonLoader/releases)
2. Extract and run the installer
3. Select your Operation New Earth game directory
4. Follow the installation wizard

### 2. Install AASmasher Mod
1. Build the mod (see above) or download the release
2. Copy `AASmasher.dll` to your game's `Mods` folder:
   ```
   [Game Directory]\Mods\AASmasher.dll
   ```
3. The `Mods` folder should be created automatically after installing MelonLoader

### 3. Launch the Game
- Start Operation New Earth normally
- MelonLoader will automatically load the AASmasher mod
- You should see mod loading messages in the MelonLoader console

## Default Game Directory Locations

- **Steam**: `C:\Program Files (x86)\Steam\steamapps\common\Operation New Earth\`
- **Epic Games**: `C:\Program Files\Epic Games\Operation New Earth\`

## Usage

Once installed and loaded:
- The mod will automatically initialize when the game starts
- Use the in-game GUI to access mod features
- Settings are automatically saved and loaded
- Check the MelonLoader console for mod status and debug information

## Configuration

The mod saves its configuration automatically. Settings can be modified through the in-game interface or by editing the generated config files in the MelonLoader UserData directory.

## Troubleshooting

### Build Issues
- **Missing References**: Run `dotnet restore` to download NuGet packages
- **MelonLoader Not Found**: Update the MelonLoader reference path in the project file
- **Build Errors**: Ensure you have .NET Framework 4.8 SDK installed

### Runtime Issues
- **Mod Not Loading**: Check that MelonLoader is properly installed
- **Game Crashes**: Verify mod compatibility with your game version
- **Missing Features**: Check MelonLoader console for error messages

## Project Structure

```
AASmasher/
├── AASmasherClass.cs          # Main mod class
├── ButtonAutomation.cs        # UI automation features
├── CameraController.cs        # Camera management
├── FatAliensManager.cs        # Special alien handling
├── GuiRenderer.cs            # ImGui rendering
├── GuiSystem.cs              # GUI system management
├── InputHandler.cs           # Input processing
├── InvasionManager.cs        # Invasion event handling
├── NightModeManager.cs       # Night vision features
├── SettingsManager.cs        # Configuration management
└── Properties/
    └── AssemblyInfo.cs       # Assembly information
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This mod is provided as-is for educational and entertainment purposes. Please respect the game's terms of service and the MelonLoader project guidelines.

## Support

- **Issues**: Report bugs and feature requests on the GitHub Issues page
- **Discord**: Join the Operation New Earth modding community
- **MelonLoader**: Visit the [MelonLoader Discord](https://discord.gg/2Wn3N2P) for general modding help

---

**Note**: This mod requires MelonLoader and is designed specifically for Operation New Earth. Make sure to keep backups of your save files before using any mods. 
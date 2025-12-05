# Adofai-Resource-Packager

Adofai-Resource-Packager is a WPF utility that reads ADOFAI chart files, automatically locates referenced audio and image assets in the same directory, and bundles everything into a single zip archive. It helps prevent missing files when distributing or backing up charts.

## Features
- Detects referenced resources (audio, images, etc.) located alongside the `.adofai` file.
- Generates a zip archive that includes both the chart and its referenced assets.
- GUI workflow for selecting the chart file and choosing the save location.
- Recreates the archive on overwrite by removing any existing zip first.

## Requirements
- Windows 10 or later (WPF application).
- [.NET 8 SDK](https://dotnet.microsoft.com/download) or runtime capable of running `net8.0-windows` apps.

## Installation / Run
1. Clone the repository.
   ```bash
   git clone https://github.com/<your-account>/Adofai-Resource-Packager.git
   cd Adofai-Resource-Packager
   ```
2. Build with .NET 8 SDK installed.
   ```bash
   dotnet build
   ```
3. To run from the development environment:
   ```bash
   dotnet run --project Adofai-Resource-Packager/Adofai-Resource-Packager.csproj
   ```
   You can also launch the generated `Adofai-Resource-Packager.exe` under the `bin/` directory directly.

## Usage
1. Launch the app and click **Load File** to select the target `.adofai` chart (place referenced resources in the same folder).
2. Click **Process**. The tool scans the chart for referenced files and shows a save dialog to create the bundled zip.
3. Choose the output location and filename (defaults to `<chart_name>_resources.zip`) and save to generate the archive.

## Behavior Notes
- Only files in the same directory as the `.adofai` chart that are mentioned in the chart text are added to the zip.
- If duplicate filenames would appear inside the archive, duplicates are skipped to avoid multiple entries.
- The `.adofai` chart itself is always included in the archive.

## License
This project is released under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

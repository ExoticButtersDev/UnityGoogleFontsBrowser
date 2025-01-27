# Google Fonts Browser for Unity

A custom Unity Editor tool to browse, download, and install Google Fonts into your Unity project. This tool integrates with the Google Fonts API, allowing you to easily browse a selection of fonts, install them, and create TextMeshPro font assets within your Unity project.

## Features
- Browse and search for fonts from the Google Fonts library.
- Download and install selected fonts into your Unity project.
- Automatically create TextMeshPro font assets for the installed fonts.
- Easily uninstall installed fonts from the Unity Editor.
- Pagination support for large font lists.

## Requirements
- Unity (Tested on Unity 2020+)
- TextMeshPro package installed in your Unity project.

## Installation

1. Clone or download this repository to your Unity project.
2. Open the `GoogleFontsBrowser.cs` script and place it under the `Assets/Editor` folder in your Unity project.
3. Ensure you have the TextMeshPro package installed. If not, install it from the Unity Package Manager.
4. You may need to set up an API key for the Google Fonts API. For that, you can replace the `your-api-key-here` key in the `GoogleFontsAPIUrl` string with your own key.
   - [Get your API key here](https://console.developers.google.com/).

## Usage

1. After importing the script into your project, open the Google Fonts Browser from the Unity Editor:
   - `Window > Exotic Butters > Tools > Google Fonts Browser`.
   
2. The window will display a list of fonts retrieved from the Google Fonts API. You can:
   - **Search** for fonts by name.
   - **Download and install** selected fonts into your project.
   - **Uninstall** any installed fonts by clicking the "Uninstall" button next to the font.

3. You can navigate through pages of fonts using the "Previous Page" and "Next Page" buttons.

4. Installed fonts will be saved and remembered across sessions. To uninstall a font, click the "Uninstall" button next to it.

## How It Works
- **Font Download**: The script fetches font data from the Google Fonts API, downloading the TTF font files and saving them under `Assets/Fonts/`.
- **TextMeshPro Integration**: When a font is downloaded, a corresponding TextMeshPro font asset is created and saved.
- **Installed Fonts**: The list of installed fonts is stored using Unity's `PlayerPrefs` so that it persists across editor sessions.

## Contributing
Feel free to fork the repository and submit pull requests. Any contributions are welcome!

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer
This tool makes use of the Google Fonts API and is subject to their terms of use. You will need a valid API key for this tool to work properly.

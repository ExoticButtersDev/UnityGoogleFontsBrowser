namespace ExoticButters.FontBrowser
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using TMPro;

    public class GoogleFontsBrowser : EditorWindow
    {
        private const string GoogleFontsAPIUrl = "https://www.googleapis.com/webfonts/v1/webfonts?key=your-api-key-here";
        private FontList fontsList;
        private List<string> fontNames;
        private List<FontInfo> displayedFonts;
        private HashSet<string> installedFonts;
        private string searchQuery = "";
        private bool isLoading = false;
        private int selectedFontIndex = 0;

        private int currentPage = 0;
        //maybe ill add a settings page for that to be able to configure
        private const int fontsPerPage = 10;

        [MenuItem("Exotic Butters/Tools/Google Fonts Browser")]
        public static void ShowWindow()
        {
            GetWindow<GoogleFontsBrowser>("Google Fonts Browser");
        }

        private async void OnEnable()
        {
            installedFonts = new HashSet<string>(PlayerPrefs.GetString("InstalledFonts", "").Split('|'));
            await FetchGoogleFonts();
        }

        private async Task FetchGoogleFonts()
        {
            isLoading = true;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync(GoogleFontsAPIUrl);
                    fontsList = JsonUtility.FromJson<FontList>(response);

                    if (fontsList.items == null || fontsList.items.Count == 0)
                    {
                        Debug.LogError("No fonts retrieved from API.");
                        return;
                    }

                    displayedFonts = fontsList.items;
                    fontNames = new List<string>();

                    foreach (var font in displayedFonts)
                    {
                        fontNames.Add(font.family);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error fetching fonts: " + ex.Message);
                }
            }
            isLoading = false;
            Repaint();
        }

        private async void DownloadAndInstallFont(FontInfo fontInfo)
        {
            string downloadUrl = fontInfo.files.regular;
            string folderPath = Path.Combine(Application.dataPath, "Fonts");
            string filePath = Path.Combine(folderPath, fontInfo.family + ".ttf");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var fontData = await client.GetByteArrayAsync(downloadUrl);
                    File.WriteAllBytes(filePath, fontData);

                    string relativeFilePath = "Assets" + filePath.Substring(Application.dataPath.Length);
                    AssetDatabase.Refresh();

                    Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(relativeFilePath);
                    if (sourceFont == null)
                    {
                        Debug.LogError($"Failed to load font at {relativeFilePath}. Ensure the file exists and is a valid font file.");
                        return;
                    }

                    TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
                    if (fontAsset == null)
                    {
                        Debug.LogError($"Failed to create TextMeshPro Font Asset for {fontInfo.family}");
                        return;
                    }

                    string fontAssetFolder = "Assets/Fonts/TextMeshPro";
                    if (!AssetDatabase.IsValidFolder(fontAssetFolder))
                    {
                        AssetDatabase.CreateFolder("Assets/Fonts", "TextMeshPro");
                    }

                    string assetPath = $"{fontAssetFolder}/{fontInfo.family}.asset";
                    AssetDatabase.CreateAsset(fontAsset, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    installedFonts.Add(fontInfo.family);
                    PlayerPrefs.SetString("InstalledFonts", string.Join("|", installedFonts));
                    PlayerPrefs.Save();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error downloading or converting font {fontInfo.family}: {ex.Message}");
                }
            }
        }

        private void UninstallFont(string fontName)
        {
            string fontPath = Path.Combine(Application.dataPath, "Fonts", fontName + ".ttf");
            string fontMetaPath = fontPath + ".meta";
            string assetPath = Path.Combine("Assets", "Fonts", "TextMeshPro", fontName + ".asset");
            string assetMetaPath = assetPath + ".meta";

            try
            {
                if (File.Exists(fontPath))
                {
                    File.Delete(fontPath);
                }
                if (File.Exists(fontMetaPath))
                {
                    File.Delete(fontMetaPath);
                }
                if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
                if (File.Exists(assetMetaPath))
                {
                    File.Delete(assetMetaPath);
                }

                AssetDatabase.Refresh();
                installedFonts.Remove(fontName);
                PlayerPrefs.SetString("InstalledFonts", string.Join("|", installedFonts));
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error uninstalling font {fontName}: {ex.Message}");
            }
        }


        private void OnGUI()
        {
            if (isLoading)
            {
                EditorGUILayout.LabelField("Loading fonts...");
                return;
            }

            GUILayout.Label("Google Fonts Browser", EditorStyles.boldLabel);
            GUILayout.Label("Scripted by ExoticButters", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            searchQuery = EditorGUILayout.TextField("Search Fonts", searchQuery);
            if (GUILayout.Button("Search"))
            {
                SearchFonts();
            }
            EditorGUILayout.EndHorizontal();

            int totalFonts = displayedFonts.Count;
            int totalPages = Mathf.CeilToInt((float)totalFonts / fontsPerPage);
            int startIndex = currentPage * fontsPerPage;
            int endIndex = Mathf.Min(startIndex + fontsPerPage, totalFonts);

            for (int i = startIndex; i < endIndex; i++)
            {
                FontInfo font = displayedFonts[i];
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(font.family);

                if (installedFonts.Contains(font.family))
                {
                    GUILayout.Label("Installed");
                }
                else
                {
                    if (GUILayout.Button("Download and Install"))
                    {
                        DownloadAndInstallFont(font);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous Page") && currentPage > 0)
            {
                currentPage--;
            }

            GUILayout.Label($"Page {currentPage + 1} of {totalPages}");

            if (GUILayout.Button("Next Page") && currentPage < totalPages - 1)
            {
                currentPage++;
            }
            EditorGUILayout.EndHorizontal();

            if (totalFonts == 0)
            {
                GUILayout.Label("No fonts Found");
            }

            GUILayout.Space(20);
            GUILayout.Label("Installed Fonts", EditorStyles.boldLabel);
            
            List<string> fontsToRemove = new List<string>();

            foreach (string fontName in installedFonts)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(fontName);

                if (GUILayout.Button("Uninstall"))
                {
                    fontsToRemove.Add(fontName);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (installedFonts.Count == 0)
            {
                GUILayout.Label("No fonts installed.");
            }

            foreach (string fontToRemove in fontsToRemove)
            {
                UninstallFont(fontToRemove);
            }
        }


        private void SearchFonts()
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                displayedFonts = fontsList.items;
            }
            else
            {
                displayedFonts = fontsList.items.FindAll(font => font.family.ToLower().Contains(searchQuery.ToLower()));
            }

            fontNames.Clear();
            foreach (var font in displayedFonts)
            {
                fontNames.Add(font.family);
            }

            currentPage = 0;
            Repaint();
        }

        [System.Serializable]
        public class FontInfo
        {
            public string family;
            public FontFiles files;
        }

        [System.Serializable]
        public class FontFiles
        {
            public string regular;
        }

        [System.Serializable]
        public class FontList
        {
            public List<FontInfo> items;
        }
    }
}

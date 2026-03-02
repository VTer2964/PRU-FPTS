#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CountryGuessGame.Data;
using System.IO;

namespace CountryGuessGame.Editor
{
    /// <summary>
    /// Editor window to help populate the Country Database with country data
    /// </summary>
    public class CountryDatabasePopulator : EditorWindow
    {
        private CountryDatabase database;
        private Vector2 scrollPosition;

        [MenuItem("Country Guess Game/Populate Country Database")]
        public static void ShowWindow()
        {
            GetWindow<CountryDatabasePopulator>("Country Database Populator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Country Database Populator", EditorStyles.boldLabel);
            
            database = (CountryDatabase)EditorGUILayout.ObjectField(
                "Country Database", 
                database, 
                typeof(CountryDatabase), 
                false
            );

            if (database == null)
            {
                EditorGUILayout.HelpBox("Please assign a Country Database asset", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Populate with 18 Countries (Available Flags)"))
            {
                PopulateWithAvailableFlags();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This will create CountryData assets for the 18 countries with generated flag images.\n" +
                "Flags will be automatically linked if found in Assets/CountryGuessGame/Sprites/Flags/",
                MessageType.Info
            );

            EditorGUILayout.Space();
            GUILayout.Label($"Current Database has {database.AllCountries.Count} countries", EditorStyles.helpBox);
        }

        private void PopulateWithAvailableFlags()
        {
            if (database == null) return;

            // Clear existing countries
            database.AllCountries.Clear();

            // Country data: Name (English), Name (Vietnamese), Difficulty, Region, Capital
            var countriesData = new[]
            {
                new { NameEn = "Vietnam", NameVi = "Việt Nam", Diff = 1, Region = "Asia", Capital = "Hanoi", FlagFile = "vietnam_flag" },
                new { NameEn = "United States", NameVi = "Hoa Kỳ", Diff = 1, Region = "Americas", Capital = "Washington D.C.", FlagFile = "usa_flag" },
                new { NameEn = "Japan", NameVi = "Nhật Bản", Diff = 1, Region = "Asia", Capital = "Tokyo", FlagFile = "japan_flag" },
                new { NameEn = "China", NameVi = "Trung Quốc", Diff = 1, Region = "Asia", Capital = "Beijing", FlagFile = "china_flag" },
                new { NameEn = "South Korea", NameVi = "Hàn Quốc", Diff = 2, Region = "Asia", Capital = "Seoul", FlagFile = "south_korea_flag" },
                new { NameEn = "Thailand", NameVi = "Thái Lan", Diff = 1, Region = "Asia", Capital = "Bangkok", FlagFile = "thailand_flag" },
                new { NameEn = "Singapore", NameVi = "Singapore", Diff = 2, Region = "Asia", Capital = "Singapore", FlagFile = "singapore_flag" },
                new { NameEn = "Malaysia", NameVi = "Malaysia", Diff = 2, Region = "Asia", Capital = "Kuala Lumpur", FlagFile = "malaysia_flag" },
                new { NameEn = "Indonesia", NameVi = "Indonesia", Diff = 1, Region = "Asia", Capital = "Jakarta", FlagFile = "indonesia_flag" },
                new { NameEn = "Philippines", NameVi = "Philippines", Diff = 2, Region = "Asia", Capital = "Manila", FlagFile = "philippines_flag" },
                new { NameEn = "United Kingdom", NameVi = "Vương quốc Anh", Diff = 1, Region = "Europe", Capital = "London", FlagFile = "uk_flag" },
                new { NameEn = "France", NameVi = "Pháp", Diff = 1, Region = "Europe", Capital = "Paris", FlagFile = "france_flag" },
                new { NameEn = "Germany", NameVi = "Đức", Diff = 1, Region = "Europe", Capital = "Berlin", FlagFile = "germany_flag" },
                new { NameEn = "Italy", NameVi = "Ý", Diff = 1, Region = "Europe", Capital = "Rome", FlagFile = "italy_flag" },
                new { NameEn = "Spain", NameVi = "Tây Ban Nha", Diff = 1, Region = "Europe", Capital = "Madrid", FlagFile = "spain_flag" },
                new { NameEn = "Brazil", NameVi = "Brazil", Diff = 1, Region = "Americas", Capital = "Brasilia", FlagFile = "brazil_flag" },
                new { NameEn = "Argentina", NameVi = "Argentina", Diff = 2, Region = "Americas", Capital = "Buenos Aires", FlagFile = "argentina_flag" },
                // Note: Mexico flag generation failed due to quota
            };

            string basePath = "Assets/CountryGuessGame/Data/Countries";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                string parentPath = "Assets/CountryGuessGame/Data";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    AssetDatabase.CreateFolder("Assets/CountryGuessGame", "Data");
                }
                AssetDatabase.CreateFolder(parentPath, "Countries");
            }

            foreach (var countryInfo in countriesData)
            {
                // Create CountryData asset
                CountryData country = ScriptableObject.CreateInstance<CountryData>();
                country.Setup(
                    countryInfo.NameEn,
                    countryInfo.NameVi,
                    countryInfo.Diff,
                    countryInfo.Region,
                    countryInfo.Capital
                );

                // Try to find and assign flag sprite
                string flagPath = $"Assets/CountryGuessGame/Sprites/Flags/{countryInfo.FlagFile}";
                string[] possibleExtensions = { ".png", "_*.png" };
                
                Sprite flagSprite = null;
                foreach (var ext in possibleExtensions)
                {
                    string[] guids = AssetDatabase.FindAssets($"{countryInfo.FlagFile} t:Texture2D", new[] { "Assets/CountryGuessGame/Sprites/Flags" });
                    if (guids.Length > 0)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                        
                        // Get sprite from texture
                        string spritePath = assetPath;
                        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                        foreach (var obj in sprites)
                        {
                            if (obj is Sprite sprite)
                            {
                                flagSprite = sprite;
                                break;
                            }
                        }
                        
                        if (flagSprite != null)
                        {
                            country.SetFlagSprite(flagSprite);
                            break;
                        }
                    }
                }

                // Save asset
                string countryAssetPath = $"{basePath}/{countryInfo.NameEn.Replace(" ", "")}.asset";
                AssetDatabase.CreateAsset(country, countryAssetPath);

                // Add to database
                database.AddCountry(country);

                Debug.Log($"Created country: {countryInfo.NameEn} with flag: {(flagSprite != null ? "Assigned" : "Missing")}");
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Country Database populated with {countriesData.Length} countries!");
        }
    }
}
#endif

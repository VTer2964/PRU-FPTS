#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CountryGuessGame.Data;

namespace CountryGuessGame.Editor
{
    /// <summary>
    /// Editor script to create the Country Database asset
    /// </summary>
    public class CountryDatabaseCreator
    {
        [MenuItem("Country Guess Game/Create Country Database")]
        public static void CreateDatabase()
        {
            string basePath = "Assets/CountryGuessGame/Data";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/CountryGuessGame", "Data");
            }

            string assetPath = $"{basePath}/CountryDatabase.asset";

            // Check if already exists
            CountryDatabase existing = AssetDatabase.LoadAssetAtPath<CountryDatabase>(assetPath);
            if (existing != null)
            {
                Debug.LogWarning("Country Database already exists at " + assetPath);
                Selection.activeObject = existing;
                return;
            }

            // Create new database
            CountryDatabase database = ScriptableObject.CreateInstance<CountryDatabase>();
            AssetDatabase.CreateAsset(database, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = database;

            Debug.Log($"Country Database created at {assetPath}");
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace StackTower
{
    /// <summary>
    /// Creates all StackTower ScriptableObject assets.
    /// Run via Window > StackTower > Create All Assets.
    /// </summary>
    public static class StackTowerAssetCreator
    {
        [MenuItem("Window/StackTower/Create All Assets")]
        public static void CreateAll()
        {
            CreateSettings();
            CreateAllLevelData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done",
                "Created StackTowerSettings + 5 LevelData assets in Assets/StackTower/Settings/",
                "OK");
        }

        [MenuItem("Window/StackTower/Create Settings")]
        public static void CreateSettings()
        {
            EnsureFolder("Assets/StackTower/Settings");
            const string path = "Assets/StackTower/Settings/StackTowerSettings.asset";
            if (AssetDatabase.LoadAssetAtPath<StackTowerSettings>(path) != null) return;

            var s = ScriptableObject.CreateInstance<StackTowerSettings>();
            AssetDatabase.CreateAsset(s, path);
            Debug.Log("[StackTower] Created " + path);
        }

        [MenuItem("Window/StackTower/Create All Level Data")]
        public static void CreateAllLevelData()
        {
            EnsureFolder("Assets/StackTower/Settings");
            Make(1, "Level 1: Tap lam quen",  10, 3.0f, 2.0f, 8f,  0.15f, 0.10f, 5,
                new[]{ C(0.3f,0.7f,1f), C(0.2f,0.85f,0.85f), C(0.4f,0.9f,0.6f) });
            Make(2, "Level 2: Vao nghe",       15, 2.5f, 2.5f, 10f, 0.13f, 0.15f, 5,
                new[]{ C(0.6f,0.4f,1f), C(0.8f,0.3f,0.9f), C(0.5f,0.5f,1f) });
            Make(3, "Level 3: Thu thach",      20, 2.5f, 3.5f, 12f, 0.12f, 0.20f, 4,
                new[]{ C(1f,0.7f,0.2f), C(1f,0.5f,0.1f), C(0.9f,0.9f,0.1f) });
            Make(4, "Level 4: Chuyen gia",     25, 2.0f, 4.5f, 14f, 0.10f, 0.25f, 4,
                new[]{ C(1f,0.3f,0.3f), C(0.9f,0.2f,0.5f), C(1f,0.1f,0.1f) });
            Make(5, "Level 5: Huyen thoai",    30, 2.0f, 5.5f, 16f, 0.08f, 0.30f, 3,
                new[]{ C(0.9f,0.1f,0.5f), C(1f,0.5f,0f), C(1f,0.8f,0f) });
        }

        private static void Make(int num, string name, int floors,
            float size, float speed, float maxSpeed,
            float perfect, float inc, int interval, Color[] colors)
        {
            string path = $"Assets/StackTower/Settings/Level{num}Data.asset";
            if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
            {
                Debug.Log($"[StackTower] {path} already exists — skip.");
                return;
            }

            var ld = ScriptableObject.CreateInstance<LevelData>();
            ld.levelNumber           = num;
            ld.levelName             = name;
            ld.targetFloors          = floors;
            ld.initialBlockSize      = size;
            ld.initialMoveSpeed      = speed;
            ld.maxMoveSpeed          = maxSpeed;
            ld.perfectThreshold      = perfect;
            ld.speedIncreaseAmount   = inc;
            ld.speedIncreaseInterval = interval;
            ld.blockColors           = colors;

            AssetDatabase.CreateAsset(ld, path);
            Debug.Log($"[StackTower] Created {path}");
        }

        private static Color C(float r, float g, float b) => new Color(r, g, b);

        private static void EnsureFolder(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }
    }
}
#endif

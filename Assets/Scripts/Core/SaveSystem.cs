using System.IO;
using UnityEngine;

namespace FPTSim.Core
{
    public static class SaveSystem
    {
        private const string FileName = "save.json";

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        public static void Save(GameState state)
        {
            if (state == null) return;

            try
            {
                string json = JsonUtility.ToJson(state, prettyPrint: true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e.Message}\nPath: {SavePath}");
            }
        }

        public static bool TryLoad(out GameState loaded)
        {
            loaded = null;

            if (!File.Exists(SavePath))
                return false;

            try
            {
                string json = File.ReadAllText(SavePath);
                loaded = JsonUtility.FromJson<GameState>(json);

                // phòng trường hợp file bị hỏng / json rỗng
                return loaded != null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed: {e.Message}\nPath: {SavePath}");
                return false;
            }
        }

        // tiện debug
        public static string GetSavePath() => SavePath;
    }
}
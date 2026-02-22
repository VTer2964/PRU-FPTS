using System.IO;
using UnityEngine;

namespace FPTSim.Core
{
    public static class SaveSystem
    {
        private const string FileName = "fptsim_save.json";

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(GameState state)
        {
            try
            {
                var json = JsonUtility.ToJson(state, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public static bool TryLoad(out GameState state)
        {
            state = null;
            try
            {
                if (!File.Exists(SavePath)) return false;
                var json = File.ReadAllText(SavePath);
                state = JsonUtility.FromJson<GameState>(json);
                return state != null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return false;
            }
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
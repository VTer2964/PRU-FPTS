using UnityEngine;

namespace FPTSim.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private GameConfigSO config;

        private void Start()
        {
            // Create GameManager if not exists
            if (GameManager.I == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }

            if (config == null)
            {
                Debug.LogError("Bootstrapper: Missing GameConfigSO reference!");
                return;
            }

            // Load save or start new
            GameManager.I.LoadOrNew(config);
        }
    }
}
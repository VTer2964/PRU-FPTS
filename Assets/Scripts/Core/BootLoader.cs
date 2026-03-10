using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPTSim.Core
{
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private GameConfigSO config;

        private void Awake()
        {
            // 1) đảm bảo có GameManager
            if (GameManager.I == null)
            {
                var gmGo = new GameObject("GameManager");
                gmGo.AddComponent<GameManager>();
                DontDestroyOnLoad(gmGo);
            }

            // 2) init config
            GameManager.I.Init(config);

            // 3) load MainMenu
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
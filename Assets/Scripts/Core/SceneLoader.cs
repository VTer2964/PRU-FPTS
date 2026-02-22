using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPTSim.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
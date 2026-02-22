using UnityEngine;

namespace FPTSim.Minigames
{
    [CreateAssetMenu(menuName = "FPT Sim/Minigame Info", fileName = "MinigameInfo")]
    public class MinigameInfoSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public string sceneName;
        [TextArea] public string description;
    }
}
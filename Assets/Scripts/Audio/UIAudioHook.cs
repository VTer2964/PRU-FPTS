using UnityEngine;
using UnityEngine.UI;
using FPTSim.Audio;

namespace FPTSim.Audio
{
    public class UIAudioHook : MonoBehaviour
    {
        [SerializeField] private AudioClip click;

        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    if (AudioManager.I != null) AudioManager.I.PlayUI(click);
                });
        }
    }
}
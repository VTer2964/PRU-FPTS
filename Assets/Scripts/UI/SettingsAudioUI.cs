using UnityEngine;
using UnityEngine.UI;
using FPTSim.Audio;

namespace FPTSim.UI
{
    public class SettingsAudioUI : MonoBehaviour
    {
        [SerializeField] private Slider master;
        [SerializeField] private Slider music;
        [SerializeField] private Slider sfx;
        [SerializeField] private Slider ui;
        [SerializeField] private Slider dialogue;
        [SerializeField] private Slider ambient;

        private void OnEnable()
        {
            if (AudioManager.I == null) return;

            // set slider theo giá trị đã lưu
            if (master) master.value = AudioManager.I.GetVolume01("MasterVol", 1f);
            if (music) music.value = AudioManager.I.GetVolume01("MusicVol", 1f);
            if (sfx) sfx.value = AudioManager.I.GetVolume01("SfxVol", 1f);
            if (ui) ui.value = AudioManager.I.GetVolume01("UiVol", 1f);
            if (dialogue) dialogue.value = AudioManager.I.GetVolume01("DialogueVol", 1f);
            if (ambient) ambient.value = AudioManager.I.GetVolume01("AmbientVol", 1f);

            // bind
            if (master) master.onValueChanged.AddListener(v => AudioManager.I.SetVolume("MasterVol", v));
            if (music) music.onValueChanged.AddListener(v => AudioManager.I.SetVolume("MusicVol", v));
            if (sfx) sfx.onValueChanged.AddListener(v => AudioManager.I.SetVolume("SfxVol", v));
            if (ui) ui.onValueChanged.AddListener(v => AudioManager.I.SetVolume("UiVol", v));
            if (dialogue) dialogue.onValueChanged.AddListener(v => AudioManager.I.SetVolume("DialogueVol", v));
            if (ambient) ambient.onValueChanged.AddListener(v => AudioManager.I.SetVolume("AmbientVol", v));
        }

        private void OnDisable()
        {
            if (master) master.onValueChanged.RemoveAllListeners();
            if (music) music.onValueChanged.RemoveAllListeners();
            if (sfx) sfx.onValueChanged.RemoveAllListeners();
            if (ui) ui.onValueChanged.RemoveAllListeners();
            if (dialogue) dialogue.onValueChanged.RemoveAllListeners();
            if (ambient) ambient.onValueChanged.RemoveAllListeners();
        }
    }
}
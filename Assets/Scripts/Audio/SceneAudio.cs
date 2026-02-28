using UnityEngine;
using FPTSim.Audio;

namespace FPTSim.Audio
{
    public class SceneAudio : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip music;
        [SerializeField] private AudioClip ambient;

        [Header("Options")]
        [SerializeField] private bool playMusicOnStart = true;
        [SerializeField] private bool playAmbientOnStart = true;

        [SerializeField] private bool stopMusicIfNull = false;
        [SerializeField] private bool stopAmbientIfNull = false;

        private void Start()
        {
            if (AudioManager.I == null) return;

            // Music
            if (playMusicOnStart)
            {
                if (music != null) AudioManager.I.PlayMusic(music, 1f, true);
                else if (stopMusicIfNull) AudioManager.I.StopMusic();
            }

            // Ambient
            if (playAmbientOnStart)
            {
                if (ambient != null) AudioManager.I.PlayAmbient(ambient, 1f, true);
                else if (stopAmbientIfNull) AudioManager.I.PlayAmbient(null); // không dùng thì bỏ
            }
        }
    }
}
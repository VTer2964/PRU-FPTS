using UnityEngine;
using UnityEngine.Audio;

namespace FPTSim.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer mixer;

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource dialogueSource;
        [SerializeField] private AudioSource ambientSource;

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            LoadVolumes();
        }

        // ---------- Play API ----------
        public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (!musicSource || clip == null) return;
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource) musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (!sfxSource || clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        public void PlayUI(AudioClip clip, float volume = 1f)
        {
            if (!uiSource || clip == null) return;
            uiSource.PlayOneShot(clip, volume);
        }

        public void PlayDialogue(AudioClip clip, float volume = 1f)
        {
            if (!dialogueSource || clip == null) return;
            dialogueSource.PlayOneShot(clip, volume);
        }

        public void PlayAmbient(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (!ambientSource || clip == null) return;
            ambientSource.clip = clip;
            ambientSource.volume = volume;
            ambientSource.loop = loop;
            ambientSource.Play();
        }

        // ---------- Volume API (0..1 -> dB) ----------
        public void SetVolume(string exposedParam, float value01)
        {
            if (!mixer) return;

            float db = (value01 <= 0.0001f) ? -80f : Mathf.Log10(value01) * 20f;
            mixer.SetFloat(exposedParam, db);

            PlayerPrefs.SetFloat(exposedParam, value01);
            PlayerPrefs.Save();
        }

        public float GetVolume01(string exposedParam, float defaultValue = 1f)
        {
            return PlayerPrefs.GetFloat(exposedParam, defaultValue);
        }

        private void LoadVolumes()
        {
            Apply("MasterVol", GetVolume01("MasterVol", 1f));
            Apply("MusicVol", GetVolume01("MusicVol", 1f));
            Apply("SfxVol", GetVolume01("SfxVol", 1f));
            Apply("UiVol", GetVolume01("UiVol", 1f));
            Apply("DialogueVol", GetVolume01("DialogueVol", 1f));
            Apply("AmbientVol", GetVolume01("AmbientVol", 1f));
        }

        private void Apply(string param, float v01)
        {
            if (!mixer) return;
            float db = (v01 <= 0.0001f) ? -80f : Mathf.Log10(v01) * 20f;
            mixer.SetFloat(param, db);
        }
    }
}
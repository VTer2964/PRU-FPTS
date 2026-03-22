using UnityEngine;
using UnityEngine.Audio;

namespace FPTSim.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public struct AudioSourceState
        {
            public AudioClip Clip;
            public float Time;
            public float Volume;
            public bool Loop;
            public bool HadClip;
            public bool ShouldResume;
        }

        public static AudioManager I { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer mixer;

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource dialogueSource;
        [SerializeField] private AudioSource dialogueVoiceSource;
        [SerializeField] private AudioSource ambientSource;

        private AudioSource runtimeDialogueVoiceSource;
        private int environmentMuteDepth;

        private float currentMusicBaseVolume = 1f;
        private float musicVolumeMultiplier = 1f;

        private void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadVolumes();
        }

        // ---------- Play API ----------
        public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (!musicSource || clip == null) return;
            currentMusicBaseVolume = volume;
            musicSource.clip = clip;
            musicSource.volume = currentMusicBaseVolume * musicVolumeMultiplier;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void SetMusicVolumeMultiplier(float multiplier)
        {
            musicVolumeMultiplier = Mathf.Clamp01(multiplier);
            if (musicSource)
            {
                musicSource.volume = currentMusicBaseVolume * musicVolumeMultiplier;
            }
        }

        public void StopMusic()
        {
            if (musicSource) musicSource.Stop();
        }

        public AudioSourceState CaptureMusicState()
        {
            return CaptureState(musicSource);
        }

        public void RestoreMusicState(AudioSourceState state)
        {
            RestoreState(musicSource, state);
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

        public void PlayDialogueVoice(AudioClip clip, float volume = 1f)
        {
            var voiceSource = GetDialogueVoiceSource();
            if (!voiceSource || clip == null) return;

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = volume;
            voiceSource.loop = false;
            voiceSource.Play();
        }

        public void StopDialogueVoice()
        {
            var voiceSource = GetDialogueVoiceSource();
            if (!voiceSource) return;

            voiceSource.Stop();
            voiceSource.clip = null;
        }

        public void PlayAmbient(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (!ambientSource || clip == null) return;
            ambientSource.clip = clip;
            ambientSource.volume = volume;
            ambientSource.loop = loop;
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            if (ambientSource) ambientSource.Stop();
        }

        public AudioSourceState CaptureAmbientState()
        {
            return CaptureState(ambientSource);
        }

        public void RestoreAmbientState(AudioSourceState state)
        {
            RestoreState(ambientSource, state);
        }

        public void PushEnvironmentMute()
        {
            environmentMuteDepth++;
            if (environmentMuteDepth != 1) return;

            PauseSource(musicSource);
            PauseSource(ambientSource);
        }

        public void PopEnvironmentMute()
        {
            if (environmentMuteDepth <= 0)
            {
                environmentMuteDepth = 0;
                return;
            }

            environmentMuteDepth--;
            if (environmentMuteDepth > 0) return;

            ResumeSource(musicSource);
            ResumeSource(ambientSource);
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

        private AudioSourceState CaptureState(AudioSource source)
        {
            if (source == null)
                return default;

            bool hadClip = source.clip != null;
            bool shouldResume = hadClip && (source.isPlaying || source.time > 0f);

            return new AudioSourceState
            {
                Clip = source.clip,
                Time = hadClip ? source.time : 0f,
                Volume = source.volume,
                Loop = source.loop,
                HadClip = hadClip,
                ShouldResume = shouldResume
            };
        }

        private void RestoreState(AudioSource source, AudioSourceState state)
        {
            if (source == null) return;

            source.Stop();
            source.clip = state.Clip;
            source.volume = state.Volume;
            source.loop = state.Loop;

            if (!state.HadClip || state.Clip == null || !state.ShouldResume)
                return;

            source.time = Mathf.Clamp(state.Time, 0f, state.Clip.length);
            source.Play();
        }

        private void PauseSource(AudioSource source)
        {
            if (source == null || !source.isPlaying) return;
            source.Pause();
        }

        private void ResumeSource(AudioSource source)
        {
            if (source == null || source.clip == null) return;
            if (source.isPlaying) return;
            if (source.time <= 0f) return;
            source.UnPause();
        }

        private AudioSource GetDialogueVoiceSource()
        {
            if (dialogueVoiceSource != null)
                return dialogueVoiceSource;

            if (runtimeDialogueVoiceSource != null)
                return runtimeDialogueVoiceSource;

            if (dialogueSource == null)
                return null;

            runtimeDialogueVoiceSource = gameObject.AddComponent<AudioSource>();
            runtimeDialogueVoiceSource.outputAudioMixerGroup = dialogueSource.outputAudioMixerGroup;
            runtimeDialogueVoiceSource.playOnAwake = false;
            runtimeDialogueVoiceSource.bypassEffects = dialogueSource.bypassEffects;
            runtimeDialogueVoiceSource.bypassListenerEffects = dialogueSource.bypassListenerEffects;
            runtimeDialogueVoiceSource.bypassReverbZones = dialogueSource.bypassReverbZones;
            runtimeDialogueVoiceSource.priority = dialogueSource.priority;
            runtimeDialogueVoiceSource.volume = dialogueSource.volume;
            runtimeDialogueVoiceSource.pitch = dialogueSource.pitch;
            runtimeDialogueVoiceSource.panStereo = dialogueSource.panStereo;
            runtimeDialogueVoiceSource.spatialBlend = dialogueSource.spatialBlend;
            runtimeDialogueVoiceSource.reverbZoneMix = dialogueSource.reverbZoneMix;
            runtimeDialogueVoiceSource.dopplerLevel = dialogueSource.dopplerLevel;
            runtimeDialogueVoiceSource.spread = dialogueSource.spread;
            runtimeDialogueVoiceSource.rolloffMode = dialogueSource.rolloffMode;
            runtimeDialogueVoiceSource.minDistance = dialogueSource.minDistance;
            runtimeDialogueVoiceSource.maxDistance = dialogueSource.maxDistance;
            runtimeDialogueVoiceSource.ignoreListenerPause = dialogueSource.ignoreListenerPause;
            runtimeDialogueVoiceSource.ignoreListenerVolume = dialogueSource.ignoreListenerVolume;
            runtimeDialogueVoiceSource.mute = dialogueSource.mute;

            return runtimeDialogueVoiceSource;
        }
    }
}

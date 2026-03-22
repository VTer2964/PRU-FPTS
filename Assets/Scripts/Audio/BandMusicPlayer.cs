using UnityEngine;

namespace FPTSim.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BandMusicPlayer : MonoBehaviour
    {
        [Header("Playlist")]
        [SerializeField] private AudioClip[] playlist;

        [Header("Playback")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loopPlaylist = true;
        [SerializeField] private bool randomStartTrack = true;
        [SerializeField][Range(0f, 1f)] private float volume = 1f;

        [Header("3D Audio")]
        [SerializeField][Range(0f, 1f)] private float spatialBlend = 1f; // 1 = 3D
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 35f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Header("Optional Mixer Route")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup outputGroup;

        [Header("BGM Ducking")]
        [SerializeField] private bool duckBGM = true;
        [SerializeField] private float duckMinDistance = 5f;
        [SerializeField] private float duckMaxDistance = 25f;
        [SerializeField][Range(0f, 1f)] private float minBGMVolumeMultiplier = 0.1f;

        private AudioSource audioSource;
        private int currentIndex = -1;
        private bool systemPaused;
        private bool stopRequested;
        private Transform listenerTransform;

        public AudioSource ManagedAudioSource => audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            // setup AudioSource
            audioSource.playOnAwake = false;
            audioSource.loop = false; // tự đổi bài bằng script
            audioSource.volume = volume;
            audioSource.spatialBlend = spatialBlend;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = rolloffMode;

            if (outputGroup != null)
                audioSource.outputAudioMixerGroup = outputGroup;
        }

        private void Start()
        {
            if (!playOnStart || playlist == null || playlist.Length == 0) return;

            if (randomStartTrack || playlist.Length == 1)
                PlayNextRandom();
            else
                PlayTrackAtIndex(0);
        }

        private void Update()
        {
            UpdateBGMDucking();

            if (playlist == null || playlist.Length == 0) return;
            if (systemPaused || stopRequested) return;

            if (!audioSource.isPlaying)
            {
                if (loopPlaylist)
                    PlayNextRandom();
            }
        }

        private void UpdateBGMDucking()
        {
            if (!duckBGM || AudioManager.I == null) return;

            if (listenerTransform == null)
            {
                var listener = Object.FindFirstObjectByType<AudioListener>();
                if (listener != null) listenerTransform = listener.transform;
            }

            if (listenerTransform != null && audioSource != null && audioSource.isPlaying)
            {
                float dist = Vector3.Distance(transform.position, listenerTransform.position);
                float t = Mathf.InverseLerp(duckMinDistance, duckMaxDistance, dist);
                float targetMultiplier = Mathf.Lerp(minBGMVolumeMultiplier, 1f, t);
                AudioManager.I.SetMusicVolumeMultiplier(targetMultiplier);
            }
            else
            {
                AudioManager.I.SetMusicVolumeMultiplier(1f);
            }
        }

        private void OnDisable()
        {
            if (duckBGM && AudioManager.I != null)
            {
                AudioManager.I.SetMusicVolumeMultiplier(1f);
            }
        }

        public void PlayNextRandom()
        {
            if (playlist == null || playlist.Length == 0) return;

            int nextIndex = GetNextRandomIndex();
            PlayTrackAtIndex(nextIndex);
        }

        public void StopMusic()
        {
            stopRequested = true;
            systemPaused = false;
            if (audioSource != null)
                audioSource.Stop();
        }

        public void PauseMusic()
        {
            if (audioSource != null)
            {
                systemPaused = true;
                audioSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (audioSource == null) return;

            stopRequested = false;
            if (!systemPaused) return;

            systemPaused = false;
            if (audioSource.clip != null)
                audioSource.UnPause();
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
                audioSource.volume = volume;
        }

        private int GetNextRandomIndex()
        {
            if (playlist.Length == 1) return 0;

            int next = Random.Range(0, playlist.Length);

            // tránh lặp lại đúng bài vừa phát
            if (next == currentIndex)
                next = (next + 1) % playlist.Length;

            return next;
        }

        private void PlayTrackAtIndex(int index)
        {
            if (playlist == null || playlist.Length == 0) return;
            if (index < 0 || index >= playlist.Length) return;

            currentIndex = index;
            stopRequested = false;
            systemPaused = false;

            AudioClip clip = playlist[currentIndex];
            if (clip == null) return;

            audioSource.clip = clip;
            audioSource.Play();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (minDistance < 0f) minDistance = 0f;
            if (maxDistance < minDistance) maxDistance = minDistance + 0.1f;
        }
#endif
    }
}

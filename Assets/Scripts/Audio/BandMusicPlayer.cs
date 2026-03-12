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

        private AudioSource audioSource;
        private int currentIndex = -1;

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
            if (playOnStart)
            {
                PlayNextRandom();
            }
        }

        private void Update()
        {
            if (playlist == null || playlist.Length == 0) return;

            if (!audioSource.isPlaying)
            {
                if (loopPlaylist)
                    PlayNextRandom();
            }
        }

        public void PlayNextRandom()
        {
            if (playlist == null || playlist.Length == 0) return;

            int nextIndex = GetNextRandomIndex();
            currentIndex = nextIndex;

            AudioClip clip = playlist[currentIndex];
            if (clip == null) return;

            audioSource.clip = clip;
            audioSource.Play();
        }

        public void StopMusic()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        public void PauseMusic()
        {
            if (audioSource != null)
                audioSource.Pause();
        }

        public void ResumeMusic()
        {
            if (audioSource != null && !audioSource.isPlaying)
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (minDistance < 0f) minDistance = 0f;
            if (maxDistance < minDistance) maxDistance = minDistance + 0.1f;
        }
#endif
    }
}
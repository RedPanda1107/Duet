using UnityEngine;

namespace Duet.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource musicSource;
        private AudioClip clickClip;
        private AudioClip crushClip;
        private AudioClip gameOverClip;
        private AudioClip backgroundMusicClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudio()
        {
            // Create two separate AudioSources - one for SFX, one for music
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();

            // Load audio clips from Resources/Audio folder
            clickClip = Resources.Load<AudioClip>("Audio/click");
            crushClip = Resources.Load<AudioClip>("Audio/crush");
            gameOverClip = Resources.Load<AudioClip>("Audio/gameover");

            // Load background music from Resources/Music folder
            backgroundMusicClip = Resources.Load<AudioClip>("Music/Broken House (juanjo_sound)");

            if (clickClip == null)
                Debug.LogWarning("Click audio clip not found in Resources/Audio folder");

            if (crushClip == null)
                Debug.LogWarning("Crush audio clip not found in Resources/Audio folder");

            if (gameOverClip == null)
                Debug.LogWarning("GameOver audio clip not found in Resources/Audio folder");

            if (backgroundMusicClip == null)
                Debug.LogWarning("Background music clip not found in Resources/Music folder");

            // Setup and play background music
            SetupBackgroundMusic();
        }

        private void SetupBackgroundMusic()
        {
            if (musicSource != null && backgroundMusicClip != null)
            {
                musicSource.clip = backgroundMusicClip;
                musicSource.loop = true;
                musicSource.volume = 0.5f;
                musicSource.Play();
                Debug.Log("Background music started playing");
            }
        }

        /// <summary>
        /// Play button click sound effect
        /// </summary>
        public void PlayClickSound()
        {
            if (clickClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clickClip);
            }
        }

        /// <summary>
        /// Play game over sound effect
        /// </summary>
        public void PlayGameOverSound()
        {
            if (gameOverClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(gameOverClip);
            }
        }

        /// <summary>
        /// Play both crush and game over sounds simultaneously (for collision/game over event)
        /// </summary>
        public void PlayCollisionGameOver()
        {
            if (crushClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(crushClip);
            }
            if (gameOverClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(gameOverClip);
            }
        }

        /// <summary>
        /// Stop background music
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Resume background music
        /// </summary>
        public void ResumeBackgroundMusic()
        {
            if (musicSource != null && !musicSource.isPlaying && backgroundMusicClip != null)
            {
                musicSource.Play();
            }
        }

        /// <summary>
        /// Set background music volume (0.0 to 1.0)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (musicSource != null)
            {
                musicSource.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>
        /// Set sound effects volume (0.0 to 1.0)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = Mathf.Clamp01(volume);
            }
        }
    }
}

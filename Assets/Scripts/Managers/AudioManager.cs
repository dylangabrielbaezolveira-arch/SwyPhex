using UnityEngine;
using System.Collections.Generic;

namespace SwyPhexLeague.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [System.Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            public float volume = 1f;
            public float pitch = 1f;
            public bool loop = false;
            public AudioSource source;
        }
        
        [Header("Music")]
        public AudioSource musicSource;
        public AudioClip[] musicTracks;
        public float musicVolume = 0.7f;
        
        [Header("SFX")]
        public Sound[] sounds;
        public float sfxVolume = 0.8f;
        
        [Header("Settings")]
        public bool mute = false;
        
        private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
        private int currentTrack = 0;
        private float originalMusicVolume;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeSounds();
            LoadAudioSettings();
        }
        
        private void Start()
        {
            PlayMusic();
        }
        
        private void InitializeSounds()
        {
            foreach (Sound sound in sounds)
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume * sfxVolume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
                
                soundDictionary[sound.name] = sound;
            }
            
            if (musicSource)
            {
                originalMusicVolume = musicSource.volume;
                musicSource.volume = musicVolume;
            }
        }
        
        private void LoadAudioSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            mute = PlayerPrefs.GetInt("Mute", 0) == 1;
            
            ApplyAudioSettings();
        }
        
        public void PlaySFX(string name, float volumeMultiplier = 1f)
        {
            if (mute || !soundDictionary.ContainsKey(name)) return;
            
            Sound sound = soundDictionary[name];
            sound.source.volume = sound.volume * sfxVolume * volumeMultiplier;
            sound.source.Play();
        }
        
        public void PlaySFXOneShot(string name, float volumeMultiplier = 1f)
        {
            if (mute || !soundDictionary.ContainsKey(name)) return;
            
            Sound sound = soundDictionary[name];
            AudioSource.PlayClipAtPoint(
                sound.clip, 
                Camera.main.transform.position, 
                sound.volume * sfxVolume * volumeMultiplier
            );
        }
        
        public void StopSFX(string name)
        {
            if (soundDictionary.ContainsKey(name))
            {
                soundDictionary[name].source.Stop();
            }
        }
        
        public void PlayMusic()
        {
            if (mute || !musicSource || musicTracks.Length == 0) return;
            
            musicSource.clip = musicTracks[currentTrack];
            musicSource.Play();
        }
        
        public void PlayNextTrack()
        {
            if (musicTracks.Length == 0) return;
            
            currentTrack = (currentTrack + 1) % musicTracks.Length;
            PlayMusic();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            
            if (musicSource)
            {
                musicSource.volume = musicVolume * originalMusicVolume;
            }
            
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            
            foreach (Sound sound in sounds)
            {
                if (sound.source)
                {
                    sound.source.volume = sound.volume * sfxVolume;
                }
            }
            
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
        
        public void SetMute(bool isMuted)
        {
            mute = isMuted;
            
            if (musicSource)
            {
                musicSource.mute = mute;
            }
            
            foreach (Sound sound in sounds)
            {
                if (sound.source)
                {
                    sound.source.mute = mute;
                }
            }
            
            PlayerPrefs.SetInt("Mute", mute ? 1 : 0);
        }
        
        public void ToggleMute()
        {
            SetMute(!mute);
        }
        
        public void PauseAll()
        {
            if (musicSource && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
            
            foreach (Sound sound in sounds)
            {
                if (sound.source && sound.source.isPlaying)
                {
                    sound.source.Pause();
                }
            }
        }
        
        public void ResumeAll()
        {
            if (musicSource && !musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
            
            foreach (Sound sound in sounds)
            {
                if (sound.source && !sound.source.isPlaying)
                {
                    sound.source.UnPause();
                }
            }
        }
        
        public void StopAll()
        {
            if (musicSource)
            {
                musicSource.Stop();
            }
            
            foreach (Sound sound in sounds)
            {
                if (sound.source)
                {
                    sound.source.Stop();
                }
            }
        }
        
        public bool IsPlaying(string name)
        {
            return soundDictionary.ContainsKey(name) && 
                   soundDictionary[name].source.isPlaying;
        }
        
        private void ApplyAudioSettings()
        {
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
            SetMute(mute);
        }
        
        public void SaveAudioSettings()
        {
            PlayerPrefs.Save();
        }
        
        private void Update()
        {
            if (musicSource && !musicSource.isPlaying && musicTracks.Length > 0)
            {
                PlayNextTrack();
            }
        }
    }
}

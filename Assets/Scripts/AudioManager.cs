using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource[] sfxSource;

    private float musicVolume;
    private float sfxVolume; 

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

        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        sfxSource = new AudioSource[5];

        for (int i = 0; i < sfxSource.Length; i++)
        {
            sfxSource[i] = gameObject.AddComponent<AudioSource>();
        }

        if (PlayerPrefs.HasKey("SettingsData"))
        {
            DataSettings saved = JsonUtility.FromJson<DataSettings>(PlayerPrefs.GetString("SettingsData"));
            musicVolume = saved.musicVolume;
            sfxVolume = saved.ambientVolume;
        }
        else
        {
            musicVolume = 1f;
            sfxVolume = 1f;
        }
        musicSource.volume = musicVolume;
    }

    public void PlayMusic(AudioClip _music, float _volume = -1f)
    {
        if (musicSource.isPlaying && musicSource.clip == _music) 
        {
            return; 
        }
        if (_volume < 0f)
        {
            _volume = musicVolume;
        }
        musicSource.clip = _music;
        musicSource.volume = _volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void FadeOutMusic(float _speed)
    {
        StartCoroutine(FadeOutAudio(musicSource, _speed));

    }
    IEnumerator FadeOutAudio (AudioSource source, float _speed)
    {
        float targetVolume = 0.15f;
        float volume = source.volume;
        while(volume>targetVolume)
        {
            volume -= Time.unscaledDeltaTime * _speed;
            source.volume = volume;
            yield return null;
        }
    }

    public void FadeOutAmbient(float _speed)
    {
        StartCoroutine(FadeOutAudio(ambientSource, _speed));
    }

    public void PlaySFX(AudioClip _sfx, float _volume = -1f)
    {
        if (_volume < 0f)
        {
            _volume = sfxVolume;
        }
        for(int i = 0; i<sfxSource.Length; i++)
        {
            if (sfxSource[i].isPlaying==false)
            {
                sfxSource[i].clip = _sfx;
                sfxSource[i].volume = _volume;
                sfxSource[i].Play();
                break;
            }
        }
    }

    public void SetMusicVolume(float _volume)
    {
        musicVolume = _volume;
        musicSource.volume = _volume;
    }

    public void SetSFXVolume(float _volume)
    {
        sfxVolume = _volume;
        for(int i = 0; i < sfxSource.Length; i++)
        {
            sfxSource[i].volume = _volume;
        }
    }
}

using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource[] sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        sfxSource = new AudioSource[5];
        for(int i = 0; i < sfxSource.Length; i++)
        {
            sfxSource[i] = gameObject.AddComponent<AudioSource>();
        }
    }
    public void PlayMusic(AudioClip _music, float _volume = 0.4f)
    {
        if (musicSource.isPlaying && musicSource.clip == _music) 
        {
            return; 
        }
        musicSource.clip = _music;
        musicSource.volume = _volume;
        musicSource.loop = true;
        musicSource.Play();
    }
    public void StopMusic()
    {
        musicSource.Stop();
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

    public void PlayAmbient(AudioClip _ambient, float _volume = 1)
    {
        ambientSource.clip = _ambient;
        ambientSource.volume = _volume;
        ambientSource.Play();
    }
    public void StopAmbient()
    {
        ambientSource.Stop();
    }
    public void FadeOutAmbient(float _speed)
    {
        StartCoroutine(FadeOutAudio(ambientSource, _speed));
    }
    public void PlaySFX(AudioClip _sfx, float _volume = 1)
    {
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
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
}

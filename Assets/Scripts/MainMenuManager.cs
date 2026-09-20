using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField]
    private Image image;
    [SerializeField]
    private float fadeSpeed = 0.5f;

    [Header("UI")]
    [SerializeField]
    private GameObject panelSettings;

    [Header("Audio")]
    [SerializeField]
    private AudioClip menuMusic;
    [SerializeField]
    private AudioClip buttonSFX;

    void Start()
    {
        AudioManager.Instance.PlayMusic(menuMusic);
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float alpha = 1.0f;
        Color colorImagen = image.color;

        while (alpha > 0)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            colorImagen.a = alpha;
            image.color = colorImagen;
            yield return null;
        }
    }

    public void PlayButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);

        // Borrar todo el progreso guardado
        PlayerPrefs.DeleteKey("SpawnX");
        PlayerPrefs.DeleteKey("SpawnY");
        PlayerPrefs.DeleteKey("CoinsCount");
        PlayerPrefs.DeleteKey("CollectedCoins");
        PlayerPrefs.DeleteKey("KillCount");
        PlayerPrefs.DeleteKey("TiempoTranscurrido");
        PlayerPrefs.DeleteKey("CamMinX");
        PlayerPrefs.DeleteKey("CamMaxX");
        PlayerPrefs.DeleteKey("CamMinY");
        PlayerPrefs.DeleteKey("CamMaxY");
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }

    public void Settings()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        panelSettings.SetActive(true);
    }

    public void BackButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        panelSettings.SetActive(false);
    }

    public void Exit()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        Application.Quit();
    }
}


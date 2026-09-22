using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Instrucciones : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField]
    private Image image;
    [SerializeField]
    private float fadeSpeed = 0.5f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip paperSFX;

    void Start()
    {
        AudioManager.Instance.PlaySFX(paperSFX);
        AudioManager.Instance.FadeOutMusic(3f);
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

    public void NextButton()
    {
        AudioManager.Instance.MiMusicVolume();
        SceneManager.LoadScene(2);
    }
}

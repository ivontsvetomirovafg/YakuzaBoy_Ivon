using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeOf : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private AudioClip fightSound;
    [SerializeField]
    private GameObject fightText;

    private PlayerController playerController;
    private static bool introPlayed; //para que no se repita todo el rato 

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController>();

        if (introPlayed == true)
        {
            image.gameObject.SetActive(false);
            playerController.canMove = true;
            return;
        }

        introPlayed = true;

        playerController.canMove = false;
        fightText.SetActive(false);

        StartCoroutine(FadeOut());
        StartCoroutine(StartSequence());
    }
    private IEnumerator FadeOut()
    {
        float alpha = 1.0f;
        Color colorImagen = image.color;

        while (alpha > 0)
        {
            alpha -= 0.05f;
            colorImagen.a = alpha;
            image.color = colorImagen;
            yield return null;
        }
    }

    private IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlaySFX(fightSound);
        fightText.SetActive(true);

        yield return new WaitForSeconds(1f);

        fightText.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        playerController.canMove = true;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Audio")]
    [SerializeField]
    private AudioClip musicSong;

    [Header("UI")]
    [SerializeField]
    private GameObject panelPause;
    [SerializeField]
    private GameObject panelLevelCompleted;

    [Header("Audio")]
    [SerializeField]
    private AudioClip pauseSFX;
    [SerializeField]
    private AudioClip victorySFX;
    [SerializeField]
    private AudioClip spawnPointSFX;
    [SerializeField]
    private AudioClip buttonSFX;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        AudioManager.Instance.PlayMusic(musicSong); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void RestartButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevelButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MainMenuButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        if (panelPause.activeInHierarchy == false)
        {
            AudioManager.Instance.PlaySFX(pauseSFX);
            panelPause.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            AudioManager.Instance.PlaySFX(buttonSFX);
            panelPause.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void FinishLevel()
    {
        AudioManager.Instance.PlaySFX(victorySFX);
        panelLevelCompleted.SetActive(true);
        Time.timeScale = 0f;
    }
}
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

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        //AudioManager.instance.PlayMusic(musicSong); // pendiente hasta que tenga el AudioManager hecho
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevelButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        if (panelPause.activeInHierarchy == false)
        {
            panelPause.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            panelPause.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void FinishLevel()
    {
        panelLevelCompleted.SetActive(true);
        Time.timeScale = 0f;
    }
}
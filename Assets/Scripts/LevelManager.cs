using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("UI")]
    [SerializeField]
    private GameObject panelPause;
    [SerializeField]
    private GameObject panelSettings;
    [SerializeField]
    private GameObject panelLevelCompleted;
    [SerializeField]
    private GameObject retryPannel;
    [SerializeField]
    private Animator victoryAnim;

    [Header("Audio")]    
    [SerializeField]
    private AudioClip musicSong;
    [SerializeField]
    private AudioClip pauseSFX;
    [SerializeField]
    private AudioClip victorySFX;
    [SerializeField]
    private AudioClip buttonSFX;

    [Header("Tiempo y puntuacion")]
    [SerializeField]
    private Text timerText;      
    [SerializeField]
    private Text winTimeText;     
    [SerializeField]
    private Text winDeathsText;  
    [SerializeField]
    private Text winPuntuacionText;   
    [SerializeField]
    private Text winNotaText;   
    [SerializeField]
    private Text winCoinsText;

    private float tiempoTrans; //cuantos seg lleva la partida.    
    private bool levelFinished;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        tiempoTrans = PlayerPrefs.GetFloat("TiempoTranscurrido", 0f);
        AudioManager.Instance.PlayMusic(musicSong); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }

        if (levelFinished == false)
        {
            tiempoTrans += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // TIEMPO Y PUNTUACI�N // 

    public void GuardarTiempoTrans()
    {
        PlayerPrefs.SetFloat("TiempoTranscurrido", tiempoTrans);
        PlayerPrefs.Save();
    }

    void UpdateTimerUI()
    {
        int minutos = Mathf.FloorToInt(tiempoTrans / 60f); 
        int segundos = Mathf.FloorToInt(tiempoTrans % 60f);
        timerText.text = minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    public void FinishLevel()
    {
        levelFinished = true;

        AudioManager.Instance.FadeOutMusic(2f);
        AudioManager.Instance.PlaySFX(victorySFX);
        panelLevelCompleted.SetActive(true);
        victoryAnim.SetTrigger("Victory");

        int killCount = PlayerPrefs.GetInt("KillCount", 0); 
        int coinsCollected = PlayerPrefs.GetInt("CoinsCount", 0); 

        float tiempoPerfecto = 360f; 
        float segundosDeMas = Mathf.Max(0f, tiempoTrans - tiempoPerfecto); // tiempo por encima del perfecto penaliza. 
        int muertesQueCuentan = Mathf.Max(0, killCount - 10); // igual con las muertes.
        int bonusMonedas = coinsCollected * 50; // cada moneda suma 50 puntos. 

        int puntuacion = Mathf.RoundToInt(10000f + bonusMonedas - (segundosDeMas * 5f) - (muertesQueCuentan * 250f)); 
        puntuacion = Mathf.Max(puntuacion, 0); // para que la puntuaci�n no baje de 0 (no num negativo). 
        string nota = GetGrade(puntuacion);

        winTimeText.text = timerText.text; 
        winDeathsText.text = "Muertes: " + killCount.ToString();
        winPuntuacionText.text = "Puntuacion: " + puntuacion.ToString();
        winNotaText.text = nota;
        winCoinsText.text = coinsCollected + "/20"; 
    }

    private string GetGrade(int score)
    {
        if (score >= 9300) 
        { 
            return "A"; 
        }

        else if (score >= 8600) 
        { 
            return "-A"; 
        }

        else if (score >= 7900) 
        { 
            return "+B"; 
        }

        else if (score >= 7200) 
        { 
            return "B"; 
        }

        else if (score >= 6500) 
        { 
            return "-B"; 
        }

        else if (score >= 5800) 
        { 
            return "+C"; 
        }

        else if (score >= 5100) 
        { 
            return "C"; 
        }

        else 
        { 
            return "-C"; 
        }
    }

    public void RestartWin()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;

        PlayerPrefs.DeleteKey("SpawnX");
        PlayerPrefs.DeleteKey("SpawnY");
        PlayerPrefs.DeleteKey("CoinsCount");
        PlayerPrefs.DeleteKey("CollectedCoins");
        PlayerPrefs.DeleteKey("TiempoTranscurrido");
        PlayerPrefs.DeleteKey("KillCount");

        PlayerPrefs.DeleteKey("CamMinX");
        PlayerPrefs.DeleteKey("CamMaxX");
        PlayerPrefs.DeleteKey("CamMinY");
        PlayerPrefs.DeleteKey("CamMaxY");

        PlayerPrefs.Save();
        FadeOf.introPlayed = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }   

    // BOTONES SETTINGS

    public void MainMenuButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;
        FadeOf.introPlayed = false;
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        if (panelPause.activeInHierarchy == false)
        {
            AudioManager.Instance.PlaySFX(pauseSFX);
            AudioManager.Instance.FadeOutMusic(2f);
            panelPause.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            AudioManager.Instance.PlaySFX(buttonSFX);
            AudioManager.Instance.MiMusicVolume();
            panelPause.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void Settings()
    {
        if (panelSettings.activeInHierarchy == false)
        {
            AudioManager.Instance.PlaySFX(buttonSFX);
            panelSettings.SetActive(true);
            panelPause.SetActive(false);
            Time.timeScale = 0f;
        }
    }

    public void RetryButton()
    {
        if (retryPannel.activeInHierarchy == false)
        {
            AudioManager.Instance.PlaySFX(pauseSFX);
            AudioManager.Instance.FadeOutMusic(2f);
            retryPannel.SetActive(true);
            panelPause.SetActive(false);
        }
    }
    public void ContinueButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        AudioManager.Instance.MiMusicVolume();
        retryPannel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void BackButton()
    {
        if (panelSettings.activeInHierarchy == true)
        {
            AudioManager.Instance.PlaySFX(buttonSFX);
            panelSettings.SetActive(false);
            panelPause.SetActive(true);
        }
    }
}
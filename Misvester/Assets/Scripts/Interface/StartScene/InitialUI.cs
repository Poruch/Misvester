using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.STP;

public class InitialUI : MonoBehaviour
{
    [SerializeField] GameObject saves;
    [SerializeField] GameObject defaultUI;
    [SerializeField] GameObject settings;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject manualButton;


    //[SerializeField] GeneralGameConfig config;
    [Header("Floating text settings")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.7f;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0, 50f);
    [SerializeField] private float moveSpeed = 30f;

    private void Awake()
    {
        FloatingTextManager.Initialize(this, textPrefab, targetCanvas, fadeInDuration, displayDuration, fadeOutDuration, Vector2.zero, moveSpeed);
    }



    GameObject CurrentActive;
    private bool DefaultUI
    {
        set
        {
            if (value)
            {
                defaultUI.SetActive(true);
                CurrentActive.SetActive(false);
                backButton.SetActive(false);
            }
            else
            {
                defaultUI.SetActive(false);
                backButton.SetActive(true);
            }
        }
    }
    private bool Saves
    {
        set
        {
            if (value)
            {
                saves.SetActive(true);
                CurrentActive = saves;
                DefaultUI = false;
            }
            else
                DefaultUI = false;
        }
    }
    private bool Settings
    {
        set
        {
            if (value)
            {
                settings.SetActive(true);
                CurrentActive = settings;
                DefaultUI = false;
            }
            else
                DefaultUI = false;
        }
    }
    public void Init()
    {
        settings.GetComponent<SettingUI>().Init();
        saves.GetComponent<SaveUI>().Init();
    }
    public void StartGame()
    {
        //config.LevelConfig.IsEndless = false;
        SceneManager.LoadScene("ScrollShooter");
    }
    public void StartEndlessGame()
    {
        //config.LevelConfig.IsEndless = true;
        SceneManager.LoadScene("ScrollShooter");
    }

    public void OpenSettings()
    {
        Settings = true;
    }

    public void OpendDefault()
    {
        DefaultUI = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenManual()
    {
        string commandText = @"Manual,_Катаргин,_Киладзе,_Мальшаков_РИС24_4.chm";
        var proc = new System.Diagnostics.Process();
        proc.StartInfo.FileName = commandText;
        proc.StartInfo.UseShellExecute = true;
        proc.Start();
    }
}

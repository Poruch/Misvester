using Assets.Scripts.Accessory;
using Assets.Scripts.GeneralGame;
using Assets.Scripts.Interface.GeneralGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GeneralGameUi : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    GameObject pauseMenu;
    [SerializeField]
    GameObject continueButton;
    [SerializeField]
    GameObject gameMenu;
    //События при открытии и закрытии интерфейса паузы
    UnityEvent onOpenUI = new UnityEvent();
    UnityEvent onCloseUI = new UnityEvent();

    UnityEvent onLoadAnimationEnd = new UnityEvent();

    [SerializeField]
    GameObject deathScreen;
    [SerializeField]
    GameObject winScreen;

    [SerializeField]
    Image loadScreen;
    [Header("Floating text settings")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.7f;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0, 50f);
    [SerializeField] private float moveSpeed = 30f;
    FloatingTextManager floatingTextManager;
    /// <summary>
    /// При передаче true открывает интерфейс, при false закрывает
    /// </summary>
    public bool IsOpen
    {
        get
        {
            return pauseMenu.activeSelf;
        }
        set
        {
            if (value)
            {
                OnOpenUI.Invoke();
                OpenPauseMenu();
            }
            else
            {
                OnCloseUI.Invoke();
                ClosePauseMenu();
            }
        }
    }
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        gameMenu.SetActive(false);
    }
    public void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        gameMenu.SetActive(true);
    }
    List<Button> pauseButtons;
    void Awake()
    {
        pauseButtons = new List<Button>(pauseMenu.GetComponentsInChildren<Button>());

        //pauseButtons[0].onClick.AddListener(Continue);
        //pauseButtons[1].onClick.AddListener(RestartGame);
        //pauseButtons[2].onClick.AddListener(ExitGame);
        //pauseButtons[3].onClick.AddListener(OpenManual);

        FloatingTextManager.Initialize(this, textPrefab, targetCanvas, fadeInDuration, displayDuration, fadeOutDuration, spawnOffset, moveSpeed);

        CloseDeathScreen();
        ClosePauseMenu();
        loadScreen.gameObject.SetActive(false);
    }


    List<TextWriter> texts = new List<TextWriter>();
    [SerializeField]
    GameObject textInput;
    public void CreateTextOutput(Func<TextWriter, string> output, int time, Vector2 coords, Vector2 size)
    {
        GameObject gameObject = Instantiate(textInput, Vector2.zero, Quaternion.identity);
        gameObject.transform.SetParent(transform, false);
        gameObject.transform.SetSiblingIndex(0);
        RectTransform pos = gameObject.GetComponent<RectTransform>();
        pos.sizeDelta = size;
        pos.anchoredPosition = coords;
        var writer = gameObject.GetComponent<TextWriter>();
        writer.Instantiate(output, TimeManager.Instance.CreateTimer(time));
        texts.Add(writer);
    }

    //[SerializeField]
    //TextMeshProUGUI fps;
    //Timer timer = TimeManager.Instance.CreateTimer(1,true);
    //private void Update()
    //{
    //    if(timer.IsTime)
    //        fps.text = ((int)(1 / Time.deltaTime)).ToString();
    //}

    //[SerializeField]
    //TextMeshProUGUI waveTime;



    UnityEvent onGameRestart = new UnityEvent();
    UnityEvent onExit = new UnityEvent();
    public void OpenManual()
    {
        string commandText = @"Manual,_Катаргин,_Киладзе,_Мальшаков_РИС24_4.chm";
        var proc = new Process();
        proc.StartInfo.FileName = commandText;
        proc.StartInfo.UseShellExecute = true;
        proc.Start();
        IsOpen = true;
    }

    [SerializeField]
    Image weaponSprite = null;
    public void SetWeaponSprite(GameObject gameObject)
    {
        weaponSprite.sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
    }


    [SerializeField]
    ImageFiller playerHeathBar = null;
    public ImageFiller PlayerHeathBar => playerHeathBar;
    [SerializeField]
    ImageFiller playerShieldBar = null;
    public ImageFiller PlayerShieldBar => playerShieldBar;
    public void OpenDeathScreen()
    {
        deathScreen.SetActive(true);
        texts.ForEach(x => x.Disable());
        pauseMenu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -150, 0);
        ClearEnemyHealthBars();
        IsOpen = true;
        //if (GameManager.isEndless)
        //{
        //    continueButton.GetComponent<Button>().interactable = false;
        //    continueButton.GetComponent<Image>().enabled = false;
        //    continueButton.GetComponentInChildren<TextMeshProUGUI>().text = GameManager.score.ToString();
        //}
        //else
        //{
        //    continueButton.gameObject.SetActive(false);
        //}
    }


    List<GameObject> enemyHealthBars = new List<GameObject>();
    [SerializeField]
    GameObject enemyHealth;
    [SerializeField]
    GameObject enemyShield;
    public ImageFiller[] CreateEnemyBars(GameObject enemy)
    {
        ImageFiller[] result = new ImageFiller[2];
        var gameObject = Instantiate(enemyHealth, this.gameObject.transform);
        gameObject.transform.SetParent(transform, false);
        gameObject.transform.SetSiblingIndex(0);
        gameObject.GetComponent<FolowObject>().Following = enemy;
        result[0] = gameObject.GetComponent<ImageFiller>();
        enemyHealthBars.Add(gameObject);

        var gameObject2 = Instantiate(enemyShield, this.gameObject.transform);
        gameObject2.transform.SetParent(transform, false);
        gameObject2.transform.SetSiblingIndex(0);
        gameObject2.GetComponent<FolowObject>().Following = enemy;
        enemyHealthBars.Add(gameObject2);
        result[1] = gameObject2.GetComponent<ImageFiller>();

        return result;
    }
    public void CloseDeathScreen()
    {
        deathScreen.SetActive(false);
        texts.ForEach(x => x.Enable());
        pauseMenu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        IsOpen = false;

        //if (GameManager.isEndless)
        //{
        //    continueButton.GetComponent<Button>().interactable = true;
        //    continueButton.GetComponent<Image>().enabled = true;
        //    continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        //}
        //else
        //{
        //    continueButton.gameObject.SetActive(true);
        //}
    }

    public void ClearEnemyHealthBars()
    {
        foreach (GameObject enemy in enemyHealthBars.Where(x => x != null))
        {
            Destroyer.Instance.Destroy(enemy);
        }
        enemyHealthBars.Clear();
    }

    public void OpenWinScreen()
    {
        winScreen.SetActive(true);
        texts.ForEach(x => x.Disable());
        pauseMenu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -150, 0);
        ClearEnemyHealthBars();
        IsOpen = true;
        //if (GameManager.isEndless)
        //{
        //    continueButton.GetComponent<Button>().interactable = false;
        //    continueButton.GetComponent<Image>().enabled = false;
        //    continueButton.GetComponentInChildren<TextMeshProUGUI>().text = GameManager.score.ToString();
        //}
        //else
        //{
        //    continueButton.gameObject.SetActive(false);
        //}
    }
    public void CloseWinScreen()
    {
        winScreen.SetActive(false);
        texts.ForEach(x => x.Enable());
        pauseMenu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        IsOpen = false;
        //if (GameManager.isEndless)
        //{
        //    continueButton.GetComponent<Button>().interactable = true;
        //    continueButton.GetComponent<Image>().enabled = true;
        //    continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        //}
        //else
        //{
        //    continueButton.gameObject.SetActive(true);
        //}

    }


    public void StartLoadAnimation(bool right)
    {
        loadScreen.gameObject.SetActive(true);
        if (right)
            loadScreen.fillOrigin = (int)Image.OriginHorizontal.Right;
        else
            loadScreen.fillOrigin = (int)Image.OriginHorizontal.Left;
        loadScreen.fillAmount = 0;
        StartCoroutine(LoadAnimation(right));
    }
    IEnumerator LoadAnimation(bool right)
    {
        //float time = 0.1f;
        int countIteration = 100;
        for (int i = 0; i <= countIteration; i++)
        {
            if (right)
                loadScreen.fillAmount = (1 - (float)i / countIteration);
            else
                loadScreen.fillAmount = (float)i / countIteration;
            yield return null;//new WaitForSeconds(time / countIteration);
        }
        loadScreen.gameObject.SetActive(false);
        onLoadAnimationEnd.Invoke();
    }

    public UnityEvent OnGameRestart { get => onGameRestart; set => onGameRestart = value; }
    public UnityEvent OnExit { get => onExit; set => onExit = value; }
    public UnityEvent OnOpenUI { get => onOpenUI; set => onOpenUI = value; }
    public UnityEvent OnCloseUI { get => onCloseUI; set => onCloseUI = value; }
    public UnityEvent OnLoadAnimationEnd { get => onLoadAnimationEnd; set => onLoadAnimationEnd = value; }

    public void RestartGame()
    {
        OnGameRestart.Invoke();
    }
    public void Continue()
    {
        IsOpen = false;
    }
    public void ExitGame()
    {
        OnExit.Invoke();
    }
}

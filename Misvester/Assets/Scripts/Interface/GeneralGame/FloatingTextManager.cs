using Assets.Scripts.Accessory;
using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingTextManager
{
    private GameObject textPrefab;
    private Canvas targetCanvas;
    private float fadeInDuration = 0.3f;
    private float displayDuration = 1.5f;
    private float fadeOutDuration = 0.7f;
    private Vector2 spawnOffset = new Vector2(0, 50f);
    private float moveSpeed = 30f;
    static FloatingTextManager instance;
    public static FloatingTextManager Instance
    {
        get
        {
            if (instance == null)
                instance = new FloatingTextManager();
            return instance;
        }
    }
    MonoBehaviour ui;
    private FloatingTextManager()
    {

    }
    public static void Initialize(MonoBehaviour ui, GameObject textPrefab, Canvas targetCanvas, float fadeInDuration, float displayDuration, float fadeOutDuration, Vector2 spawnOffset, float moveSpeed)
    {
        Instance.textPrefab = textPrefab;
        Instance.targetCanvas = targetCanvas;
        Instance.fadeInDuration = fadeInDuration;
        Instance.displayDuration = displayDuration;
        Instance.fadeOutDuration = fadeOutDuration;
        Instance.spawnOffset = spawnOffset;
        Instance.moveSpeed = moveSpeed;
        Instance.ui = ui;
    }


    public void CreateFloatingText(string message, Vector3 worldPosition, Color? color = null)
    {
        // Конвертируем мировые координаты в экранные
        float s = targetCanvas.scaleFactor;
        Vector3 screenPos = Camera.main.WorldToScreenPoint((Vector2)worldPosition + spawnOffset + new Vector2(Random.Range(0f, 0.3f), Random.Range(0f, 0.3f))) / s;
        float h = Screen.height;
        float w = Screen.width;
        float x = screenPos.x - (w / 2);
        float y = screenPos.y - (h / 2);
        // Создаем новый экземпляр текста
        GameObject textInstance = GameObject.Instantiate(textPrefab, targetCanvas.transform);
        TextMeshProUGUI textComponent = textInstance.GetComponent<TextMeshProUGUI>();

        // Настраиваем текст
        textComponent.text = message;
        textComponent.color = color ?? Color.white; // Используем белый цвет по умолчанию
        textComponent.gameObject.transform.position = (Vector2)worldPosition + spawnOffset + new Vector2(Random.Range(0f, 0.3f), Random.Range(0f, 0.3f));
        //textComponent.rectTransform.anchoredPosition =  new Vector2(x,y);

        // Запускаем анимацию
        ui.StartCoroutine(FloatAndFade(textInstance, textComponent));
    }

    private IEnumerator FloatAndFade(GameObject textInstance, TextMeshProUGUI textComponent)
    {
        // Fade-in
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha);
            textComponent.rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // Отображение
        yield return new WaitForSeconds(displayDuration);

        // Fade-out
        timer = 0f;
        Color startColor = textComponent.color;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            textComponent.rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
            yield return null;
        }

        // Уничтожаем объект после анимации
        Destroyer.Instance.Destroy(textInstance);
    }
}
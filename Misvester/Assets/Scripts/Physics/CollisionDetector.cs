// CollisionDetector.cs - только обнаружение пересечений (триггеры и зоны)
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class CollisionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] protected LayerMask detectionMask = ~0; // какие слои обнаруживать
    [SerializeField] protected bool detectTriggers = true;   // обнаруживать ли триггеры
    [SerializeField] protected bool detectNonTriggers = false; // обнаруживать ли обычные коллайдеры (для зон)

    [Header("Periodic Check")]
    [SerializeField] protected bool checkEveryFrame = true;
    [SerializeField] protected float checkInterval = 0.1f;

    [Header("Events")]
    public UnityEvent<GameObject> onTriggerEnter = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onTriggerStay = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onTriggerExit = new UnityEvent<GameObject>();

    [Header("Debug")]
    [SerializeField] protected bool showDebug = true;

    protected Collider2D myCollider;
    protected ContactFilter2D contactFilter;
    protected List<Collider2D> overlapBuffer = new List<Collider2D>(16);
    protected List<GameObject> currentInside = new List<GameObject>();
    protected float lastCheckTime;

    protected virtual void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
        {
            Debug.LogError("CollisionDetector: нет Collider2D на объекте!", this);
            return;
        }

        // Настраиваем фильтр
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(detectionMask);
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = detectTriggers;
        // Если нужно обнаруживать и обычные коллайдеры, расширяем фильтр
        if (detectNonTriggers)
        {
            // useTriggers = true означает, что мы включаем и триггеры, и обычные коллайдеры
            contactFilter.useTriggers = true;
        }
    }

    protected virtual void Start()
    {
        lastCheckTime = Time.time;
    }

    protected virtual void Update()
    {
        if (!checkEveryFrame && Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            PerformDetection();
        }
        else if (checkEveryFrame)
        {
            PerformDetection();
        }
    }

    /// <summary>
    /// Выполняет обнаружение объектов внутри коллайдера
    /// </summary>
    public void PerformDetection()
    {
        if (myCollider == null) return;

        overlapBuffer.Clear();
        int hitCount = myCollider.Overlap(contactFilter, overlapBuffer);

        List<GameObject> newlyInside = new List<GameObject>();
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = overlapBuffer[i];
            if (col == null || col.gameObject == gameObject) continue;
            newlyInside.Add(col.gameObject);
        }

        // Выход
        for (int i = currentInside.Count - 1; i >= 0; i--)
        {
            GameObject obj = currentInside[i];
            if (!newlyInside.Contains(obj))
            {
                onTriggerExit?.Invoke(obj);
                currentInside.RemoveAt(i);
            }
        }

        // Вход и пребывание
        foreach (GameObject obj in newlyInside)
        {
            if (!currentInside.Contains(obj))
            {
                onTriggerEnter?.Invoke(obj);
                currentInside.Add(obj);
            }
            else
            {
                onTriggerStay?.Invoke(obj);
            }
        }
    }

    /// <summary>
    /// Получить все объекты определённого типа внутри зоны
    /// </summary>
    public List<T> GetComponentsInside<T>() where T : Component
    {
        List<T> result = new List<T>();
        foreach (GameObject obj in currentInside)
        {
            T comp = obj.GetComponent<T>();
            if (comp != null) result.Add(comp);
        }
        return result;
    }

    /// <summary>
    /// Проверить, находится ли объект внутри зоны
    /// </summary>
    public bool IsInside(GameObject obj)
    {
        return currentInside.Contains(obj);
    }

    /// <summary>
    /// Настройка маски и типа обнаружения
    /// </summary>
    public void SetDetectionParams(LayerMask mask, bool detectTrig, bool detectNonTrig)
    {
        detectionMask = mask;
        detectTriggers = detectTrig;
        detectNonTriggers = detectNonTrig;

        contactFilter.SetLayerMask(mask);
        contactFilter.useTriggers = detectTriggers || detectNonTriggers;
    }


    // Можно также добавить методы для ручного принудительного обновления
    public void ForceUpdate() => PerformDetection();
}
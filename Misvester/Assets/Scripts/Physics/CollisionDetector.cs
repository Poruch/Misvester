// CollisionDetector.cs - только обнаружение, без физики
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Assets.Scripts.Accessory;
using System.Linq;
using UnityEngine.Rendering;

public class CollisionDetector : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] protected LayerMask collisionMask;

    [Header("Trigger Settings")]
    [SerializeField] protected bool isActiveTrigger = true;
    [SerializeField] protected bool isEveryFrame = true;
    [SerializeField] protected bool triggerOnEnter = true;
    [SerializeField] protected bool triggerOnStay = true;
    [SerializeField] protected bool triggerOnExit = true;
    [SerializeField] protected float triggerCheckInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] protected bool showDebug = true;

    protected Collider2D myCollider;
    protected ContactFilter2D contactFilter;
    protected List<Collider2D> overlapBuffer;
    protected List<GameObject> currentTriggers = new List<GameObject>();
    protected Timer triggerTimer;
    // Events - те же имена
    [Header("Events")]
    public UnityEvent<GameObject> onTriggerEnter = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onTriggerStay = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onTriggerExit = new UnityEvent<GameObject>();

    protected virtual void Awake()
    {
        myCollider = GetComponent<Collider2D>();

        // Настройка фильтра коллизий (такая же как в оригинале)
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(collisionMask);
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = false;

        // Инициализация буферов (такие же как в оригинале)
        overlapBuffer = new List<Collider2D>(16);

        triggerTimer = TimeManager.Instance.CreateTimer(triggerCheckInterval, true);
    }
    protected virtual void Start()
    {

    }
    protected virtual void Update()
    {

    }
    protected virtual void FixedUpdate()
    {
        // Периодическая статическая проверка
        if (isActiveTrigger)
        {
            if (isEveryFrame)
                PerformStaticDetection();
            else if (triggerTimer.IsTime)
            {
                PerformStaticDetection();
            }
        }
    }

    /// <summary>
    /// Выполняет рейкаст в заданном направлении, игнорируя собственный объект.
    /// </summary>
    /// <param name="origin">Точка начала луча</param>
    /// <param name="direction">Направление луча (автоматически нормализуется)</param>
    /// <param name="distance">Максимальная длина луча</param>
    /// <param name="layerMask">Маска слоёв для проверки</param>
    /// <returns>Результат рейкаста. Если попал в другой объект — данные коллизии, иначе default.</returns>
    protected RaycastHit2D RaycastIgnoreSelf(Vector2 origin, Vector2 direction, float distance = Mathf.Infinity, LayerMask layerMask = default)
    {
        if (myCollider == null)
            return default;

        // Нормализуем направление
        if (direction != Vector2.zero)
            direction = direction.normalized;
        else
            return default;

        // Используем небольшой буфер для получения всех попаданий
        RaycastHit2D[] results = new RaycastHit2D[4];
        int hitCount = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask);

        // Ищем первый хит, который НЕ принадлежит этому объекту
        for (int i = 0; i < hitCount; i++)
        {
            if (results[i].collider != null && results[i].collider.gameObject != gameObject)
            {
                return results[i];
            }
        }

        // Ничего не найдено — возвращаем "пустой" результат
        return default;
    }

    /// <summary>
    /// Проверяет, столкнётся ли объект с кем-то при движении по текущей скорости
    /// </summary>
    public bool WillCollide(Vector2 move, out RaycastHit2D hit)
    {
        hit = new RaycastHit2D();
        RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        if (move.magnitude < 0.001f)
            return false;

        // Бросаем форму коллайдера вперёд
        int hitCount = myCollider.Cast(
            move,
            contactFilter,
            hitBuffer,
            move.magnitude
        );

        if (hitCount > 0)
        {
            // Находим ближайший контакт
            hit = hitBuffer[0];
            for (int i = 1; i < hitCount; i++)
            {
                if (hitBuffer[i].distance < hit.distance)
                    hit = hitBuffer[i];
            }
            return true;
        }

        return false;
    }

    protected bool UpdateOverlap()
    {
        int hitCount = myCollider.Overlap(contactFilter, overlapBuffer);
        return hitCount != 0;
    }

    /// <summary>
    /// Статическое обнаружение объектов внутри коллайдера
    /// </summary>
    public void PerformStaticDetection()
    {
        //Просто очистка буфера
        if (overlapBuffer == null)
            overlapBuffer = new List<Collider2D>(16);

        overlapBuffer.Clear();

        if (myCollider == null)
        {
            Debug.LogWarning("Нет Collider2D для обнаружения!", this);
            return;
        }

        if (!UpdateOverlap()) return;
        // Получаем все коллайдеры внутри нашего коллайдера


        // Создаем список новых объектов
        List<GameObject> newDetectedObjects = new List<GameObject>();

        foreach (Collider2D hitCollider in overlapBuffer)
        {
            if (hitCollider != null && hitCollider != myCollider)
            {
                GameObject hitObject = hitCollider.gameObject;
                newDetectedObjects.Add(hitObject);

                // Если объект новый - вызываем событие входа
                if (!currentTriggers.Contains(hitObject))
                {
                    if (triggerOnEnter)
                    {
                        onTriggerEnter?.Invoke(hitObject);
                    }
                }
            }
        }

        // Проверяем объекты, которые вышли из области
        if (triggerOnExit)
        {
            for (int i = currentTriggers.Count - 1; i >= 0; i--)
            {
                GameObject oldObject = currentTriggers[i];
                if (!newDetectedObjects.Contains(oldObject))
                {
                    if (triggerOnExit)
                    {
                        onTriggerExit?.Invoke(oldObject);
                    }
                    currentTriggers.RemoveAt(i);
                }
            }
        }

        // Обновляем список текущих объектов
        currentTriggers = newDetectedObjects;

        // Вызываем событие со всеми текущими объектами
        for (int i = 0; i < currentTriggers.Count; i++)
            onTriggerStay?.Invoke(currentTriggers[i]);
    }

    /// <summary>
    /// Получает объекты определенного типа внутри
    /// </summary>
    public List<T> GetObjectsInsideOfType<T>() where T : Component
    {
        List<T> result = currentTriggers.Select(x => x.GetComponent<T>()).Where(x => x != null).ToList();
        return result;
    }

    /// <summary>
    /// Установка маски коллизий
    /// </summary>
    public void SetCollisionMask(LayerMask newMask)
    {
        collisionMask = newMask;
        contactFilter.SetLayerMask(newMask);
    }

    /// <summary>
    /// Настройка триггерных событий
    /// </summary>
    public void SetTriggerEvents(bool onEnter, bool onStay, bool onExit)
    {
        triggerOnEnter = onEnter;
        triggerOnStay = onStay;
        triggerOnExit = onExit;
    }
}
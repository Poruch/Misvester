using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class CollisionPredictor : MonoBehaviour
{
    public enum CollisionMode
    {
        TriggerOnly,    // Только события, без физического взаимодействия
        PushOut,        // Выталкивание из коллайдеров
    }

    [Header("Collision Settings")]
    [SerializeField] protected LayerMask collisionMask;
    [SerializeField] protected float skinWidth = 0.01f;
    [SerializeField] protected CollisionMode collisionMode = CollisionMode.PushOut;

    [Header("Push Out Settings")]
    [SerializeField] protected bool pushOutEveryFrame = true;
    [SerializeField] protected float pushOutForce = 0.1f;
    [SerializeField] protected int maxPushOutAttempts = 3;
    [SerializeField] protected bool pushOutOnStart = true;

    [Header("Trigger Settings")]
    [SerializeField] protected bool triggerOnEnter = true;
    [SerializeField] protected bool triggerOnStay = false;
    [SerializeField] protected bool triggerOnExit = true;
    [SerializeField] protected float triggerCheckInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] protected bool showDebug = true;
    [SerializeField] protected Color triggerColor = Color.yellow;
    [SerializeField] protected Color collisionColor = Color.red;
    [SerializeField] protected Color detectionColor = Color.cyan;
    [SerializeField] protected bool logPushOut = false;

    protected Rigidbody2D rb2d;
    protected Collider2D myCollider;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<GameObject> currentTriggers = new List<GameObject>();
    protected List<Collider2D> triggerOverlapBuffer;
    protected List<Collider2D> pushOutBuffer;
    protected float triggerTimer;

    protected const int maxCollisionIterations = 3;

    // Events
    [Header("Events")]
    public UnityEvent<GameObject> onTriggerEnter = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onTriggerStay = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onTriggerExit = new UnityEvent<GameObject>();
    public UnityEvent<GameObject, Vector2> onCollision = new UnityEvent<GameObject, Vector2>();
    public UnityEvent<GameObject> onPenetration = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> onPushOut = new UnityEvent<GameObject>(); // Новое событие

    protected virtual void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        // Настройка фильтра коллизий
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(collisionMask);
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = false;

        // Инициализация Rigidbody если нужно
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.simulated = true;
        rb2d.useFullKinematicContacts = true;

        // Инициализация буферов
        triggerOverlapBuffer = new List<Collider2D>(16);
        pushOutBuffer = new List<Collider2D>(16);
    }

    protected virtual void Start()
    {
        // Начальное выталкивание при старте
        if (pushOutOnStart)
        {
            PerformPushOut();
        }
    }

    protected virtual void Update()
    {
        // Периодическая статическая проверка
        if (collisionMode == CollisionMode.TriggerOnly)
        {
            triggerTimer += Time.deltaTime;
            if (triggerTimer >= triggerCheckInterval)
            {
                PerformStaticDetection();
                triggerTimer = 0f;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        // Выталкивание каждый физический кадр
        if (pushOutEveryFrame && collisionMode == CollisionMode.PushOut)
        {
            PerformPushOut();
        }
    }

    /// <summary>
    /// Выталкивание персонажа из всех коллайдеров
    /// </summary>
    public void PerformPushOut()
    {
        if (myCollider == null || rb2d == null) return;

        pushOutBuffer.Clear();

        // Получаем все коллайдеры, с которыми пересекаемся
        int overlapCount = myCollider.Overlap(contactFilter, pushOutBuffer);

        if (overlapCount == 0) return;

        bool wasPushed = false;

        // Для каждого пересечения пытаемся вытолкнуть персонажа
        foreach (Collider2D otherCollider in pushOutBuffer)
        {
            if (otherCollider == null || otherCollider == myCollider)
                continue;

            if (TryPushOutOfCollider(otherCollider))
            {
                wasPushed = true;
                onPushOut?.Invoke(otherCollider.gameObject);
            }
        }

        // Синхронизируем физику после выталкивания
        if (wasPushed)
        {
            Physics2D.SyncTransforms();
        }
    }

    /// <summary>
    /// Пытается вытолкнуть персонажа из конкретного коллайдера
    /// </summary>
    protected bool TryPushOutOfCollider(Collider2D otherCollider)
    {
        if (myCollider == null || otherCollider == null) return false;

        // Получаем расстояние между коллайдерами
        ColliderDistance2D distance = myCollider.Distance(otherCollider);

        if (!distance.isValid || distance.distance >= 0)
            return false; // Нет пересечения

        // Вычисляем направление выталкивания
        Vector2 pushDirection = GetPushOutDirection(distance);

        // Вычисляем расстояние выталкивания
        float pushDistance = Mathf.Abs(distance.distance) + pushOutForce;

        // Пробуем несколько раз с разными направлениями
        for (int attempt = 0; attempt < maxPushOutAttempts; attempt++)
        {
            if (attempt > 0)
            {
                // На следующих попытках пробуем альтернативные направления
                pushDirection = GetAlternativePushDirection(pushDirection, attempt);
            }

            if (TryMoveInDirection(pushDirection, pushDistance, otherCollider))
            {
                if (logPushOut)
                {
                    Debug.Log($"Pushed out of {otherCollider.name} with direction {pushDirection}", this);
                }
                return true;
            }
        }

        if (logPushOut)
        {
            Debug.LogWarning($"Failed to push out of {otherCollider.name} after {maxPushOutAttempts} attempts", this);
        }

        return false;
    }

    /// <summary>
    /// Получает основное направление выталкивания
    /// </summary>
    protected Vector2 GetPushOutDirection(ColliderDistance2D distance)
    {
        // Используем нормаль от расстояния между коллайдерами
        if (distance.normal != Vector2.zero)
            return distance.normal;

        // Если нормаль нулевая, используем направление от центра
        Vector2 fromCenter = (rb2d.position - (Vector2)distance.pointB).normalized;
        if (fromCenter != Vector2.zero)
            return fromCenter;

        // По умолчанию - вверх
        return Vector2.up;
    }

    /// <summary>
    /// Получает альтернативное направление для попытки
    /// </summary>
    protected Vector2 GetAlternativePushDirection(Vector2 baseDirection, int attempt)
    {
        // Набор альтернативных направлений
        Vector2[] alternativeDirections = new Vector2[]
        {
            new Vector2(-baseDirection.y, baseDirection.x).normalized,  // Перпендикуляр 1
            new Vector2(baseDirection.y, -baseDirection.x).normalized,  // Перпендикуляр 2
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        int directionIndex = attempt % alternativeDirections.Length;
        return alternativeDirections[directionIndex];
    }

    /// <summary>
    /// Пытается двигать персонажа в заданном направлении
    /// </summary>
    protected bool TryMoveInDirection(Vector2 direction, float distance, Collider2D fromCollider)
    {
        if (direction == Vector2.zero) return false;

        Vector2 originalPosition = rb2d.position;
        Vector2 targetPosition = originalPosition + direction * distance;

        // Сохраняем оригинальную позицию коллайдера
        Vector2 colliderOriginalPosition = myCollider.transform.position;

        // Временно двигаем коллайдер для проверки
        myCollider.transform.position = targetPosition;

        // Проверяем пересечения в новой позиции
        bool hasCollision = CheckCollisionAtPosition(targetPosition, fromCollider);

        // Возвращаем коллайдер
        myCollider.transform.position = colliderOriginalPosition;

        // Если в новой позиции нет коллизий - двигаем
        if (!hasCollision)
        {
            rb2d.position = targetPosition;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Проверяет коллизии в указанной позиции
    /// </summary>
    protected bool CheckCollisionAtPosition(Vector2 position, Collider2D ignoreCollider = null)
    {
        if (myCollider == null) return true;

        // Временно перемещаем коллайдер
        Vector2 originalPosition = myCollider.transform.position;
        myCollider.transform.position = position;

        // Проверяем пересечения
        List<Collider2D> overlaps = new List<Collider2D>();
        int overlapCount = myCollider.Overlap(contactFilter, overlaps);

        // Возвращаем на место
        myCollider.transform.position = originalPosition;

        // Исключаем указанный коллайдер из проверки
        if (ignoreCollider != null && overlaps.Contains(ignoreCollider))
        {
            overlapCount--;
        }

        return overlapCount > 0;
    }

    /// <summary>
    /// Статическое обнаружение объектов внутри коллайдера
    /// </summary>
    public void PerformStaticDetection()
    {
        if (triggerOverlapBuffer == null)
            triggerOverlapBuffer = new List<Collider2D>(16);

        triggerOverlapBuffer.Clear();

        if (myCollider == null)
        {
            Debug.LogWarning("Нет Collider2D для обнаружения!", this);
            return;
        }

        // Получаем все коллайдеры внутри нашего коллайдера
        int hitCount = myCollider.Overlap(contactFilter, triggerOverlapBuffer);

        // Создаем список новых объектов
        List<GameObject> newDetectedObjects = new List<GameObject>();

        foreach (Collider2D hitCollider in triggerOverlapBuffer)
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

        // Визуализация для отладки
        if (showDebug && logPushOut)
        {
            Debug.Log($"Статическое обнаружение: найдено {currentTriggers.Count} объектов");
            foreach (var obj in currentTriggers)
            {
                Debug.Log($"  - {obj.name}", obj);
            }
        }
    }

    /// <summary>
    /// Проверяет, находится ли конкретный объект внутри
    /// </summary>
    public bool IsObjectInside(GameObject targetObject)
    {
        return currentTriggers.Contains(targetObject);
    }

    /// <summary>
    /// Получает все объекты внутри области
    /// </summary>
    public List<GameObject> GetAllObjectsInside()
    {
        return new List<GameObject>(currentTriggers);
    }

    /// <summary>
    /// Получает объекты определенного типа внутри
    /// </summary>
    public List<T> GetObjectsInsideOfType<T>() where T : Component
    {
        List<T> result = new List<T>();
        foreach (GameObject obj in currentTriggers)
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                result.Add(component);
            }
        }
        return result;
    }

    /// <summary>
    /// Быстрая проверка одного объекта на вхождение
    /// </summary>
    public bool CheckSingleObject(GameObject target)
    {
        if (target == null) return false;

        if (myCollider == null)
            myCollider = GetComponent<Collider2D>();

        Collider2D targetCollider = target.GetComponent<Collider2D>();

        if (myCollider == null || targetCollider == null)
            return false;

        triggerOverlapBuffer.Clear();
        int overlapCount = myCollider.Overlap(contactFilter, triggerOverlapBuffer);
        return overlapCount > 0 && triggerOverlapBuffer.Contains(targetCollider);
    }

    /// <summary>
    /// Проверяет и разрешает столкновения
    /// </summary>
    protected bool CheckAndResolveCollision(ref Vector2 movement)
    {
        float distance = movement.magnitude;

        // Выполняем Cast для обнаружения коллизий
        int hitCount = rb2d.Cast(
            movement.normalized,
            contactFilter,
            hitBuffer,
            distance + skinWidth
        );

        if (hitCount == 0)
            return false; // Нет коллизий

        // Находим ближайшее столкновение
        RaycastHit2D closestHit = GetClosestHit(hitCount);

        // Вычисляем расстояние до столкновения
        float hitDistance = closestHit.distance - skinWidth;

        if (hitDistance < 0)
        {
            // Проникновение в коллайдер
            if (collisionMode != CollisionMode.TriggerOnly)
                HandlePenetration(closestHit);

            onPenetration?.Invoke(closestHit.collider.gameObject);
            return true;
        }

        // Обрабатываем столкновение в зависимости от режима
        switch (collisionMode)
        {
            case CollisionMode.PushOut:
                HandlePushOutCollisionDirectional(closestHit, ref movement, hitDistance);
                break;

            case CollisionMode.TriggerOnly:
                // Только события
                OnTriggerDetected(closestHit.collider.gameObject);
                break;
        }

        return true;
    }

    /// <summary>
    /// Режим: Выталкивание
    /// </summary>
    protected bool HandlePushOutCollision(RaycastHit2D hit, ref Vector2 movement, float hitDistance)
    {
        // 1. Двигаемся до точки столкновения
        Vector2 movementToHit = movement.normalized * hitDistance;
        rb2d.position += movementToHit;

        // 2. Вызываем события
        onCollision?.Invoke(hit.collider.gameObject, hit.normal);

        // 3. Вычисляем компоненты движения
        Vector2 remainingMovement = movement - movementToHit;

        // 4. Проекция оставшегося движения на нормаль столкновения
        float dot = Vector2.Dot(remainingMovement.normalized, hit.normal);

        if (Mathf.Abs(dot) > 0.001f) // Любая составляющая вдоль нормали
        {
            // Обнуляем только компоненту, направленную В коллайдер
            if (dot < 0) // Движемся внутрь
            {
                movement -= hit.normal * dot;
            }
        }

        // 5. Применяем оставшееся движение
        movement = remainingMovement;

        // 6. Небольшое трение для стабильности
        movement *= 0.9f;

        // Возвращаем true, если движение полностью остановлено
        return movement.magnitude < 0.001f;
    }

    /// <summary>
    /// Режим: Выталкивание
    /// </summary>
    protected bool HandlePushOutCollisionDirectional(RaycastHit2D hit, ref Vector2 movement, float hitDistance)
    {
        // 1. Двигаемся до точки столкновения
        Vector2 movementToHit = movement.normalized * hitDistance;
        rb2d.position += movementToHit;
        Vector2 normal = new Vector2(
            Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.y) ? hit.normal.x : 0,
            Mathf.Abs(hit.normal.y) > Mathf.Abs(hit.normal.x) ? hit.normal.y : 0
        );

        // 2. Вызываем события
        onCollision?.Invoke(hit.collider.gameObject, normal);

        // 3. Вычисляем компоненты движения
        Vector2 remainingMovement = movement - movementToHit;

        // 4. Проекция оставшегося движения на нормаль столкновения
        float dot = Vector2.Dot(remainingMovement.normalized, normal);

        if (Mathf.Abs(dot) > 0.001f) // Любая составляющая вдоль нормали
        {
            // Обнуляем только компоненту, направленную В коллайдер
            if (dot < 0) // Движемся внутрь
            {
                movement -= normal * dot;
            }
        }

        // 5. Применяем оставшееся движение
        movement = remainingMovement;

        // 6. Небольшое трение для стабильности
        movement *= 0.9f;

        // Возвращаем true, если движение полностью остановлено
        return movement.magnitude < 0.001f;
    }

    /// <summary>
    /// Обработка проникновения
    /// </summary>
    protected void HandlePenetration(RaycastHit2D hit)
    {
        // Выталкиваем объект из коллайдера
        Vector2 penetrationNormal = hit.normal;
        float penetrationDepth = skinWidth - hit.distance;

        if (penetrationDepth > 0)
            rb2d.position += penetrationNormal * penetrationDepth;
    }

    /// <summary>
    /// Триггерная проверка движения
    /// </summary>
    protected void CheckTriggerMovement(Vector2 movement)
    {
        float distance = movement.magnitude;

        if (distance < 0.001f)
            return;

        // Проверяем пересечения по пути движения
        int hitCount = rb2d.Cast(
            movement.normalized,
            contactFilter,
            hitBuffer,
            distance + skinWidth
        );

        // Обработка триггеров
        List<GameObject> newTriggers = new List<GameObject>();

        for (int i = 0; i < hitCount; i++)
        {
            GameObject hitObject = hitBuffer[i].collider.gameObject;

            if (!currentTriggers.Contains(hitObject))
            {
                // Новый триггер
                if (triggerOnEnter)
                {
                    onTriggerEnter?.Invoke(hitObject);
                    currentTriggers.Add(hitObject);
                }
            }

            newTriggers.Add(hitObject);
        }

        // Проверка выхода из триггеров
        if (triggerOnExit)
        {
            for (int i = currentTriggers.Count - 1; i >= 0; i--)
            {
                if (!newTriggers.Contains(currentTriggers[i]))
                {
                    onTriggerExit?.Invoke(currentTriggers[i]);
                    currentTriggers.RemoveAt(i);
                }
            }
        }

        // Обновляем список текущих триггеров
        if (!triggerOnExit)
        {
            currentTriggers = newTriggers;
        }
    }

    /// <summary>
    /// Обнаружение триггера
    /// </summary>
    protected void OnTriggerDetected(GameObject other)
    {
        if (triggerOnEnter && !currentTriggers.Contains(other))
        {
            onTriggerEnter?.Invoke(other);
            currentTriggers.Add(other);
        }

        if (triggerOnStay)
        {
            onTriggerStay?.Invoke(other);
        }
    }

    /// <summary>
    /// Находит ближайшее столкновение
    /// </summary>
    protected RaycastHit2D GetClosestHit(int hitCount)
    {
        if (hitCount == 0)
            return new RaycastHit2D();

        RaycastHit2D closestHit = hitBuffer[0];

        for (int i = 1; i < hitCount; i++)
        {
            if (hitBuffer[i].distance < closestHit.distance)
                closestHit = hitBuffer[i];
        }

        return closestHit;
    }

    /// <summary>
    /// Установка режима коллизий
    /// </summary>
    public void SetCollisionMode(CollisionMode mode)
    {
        collisionMode = mode;
    }

    /// <summary>
    /// Включение/выключение выталкивания
    /// </summary>
    public void EnablePushOut(bool enable)
    {
        collisionMode = enable ? CollisionMode.PushOut : CollisionMode.TriggerOnly;
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

    /// <summary>
    /// Очистка текущих триггеров
    /// </summary>
    public void ClearTriggers()
    {
        if (triggerOnExit)
        {
            foreach (var trigger in currentTriggers)
            {
                onTriggerExit?.Invoke(trigger);
            }
        }
        currentTriggers.Clear();
    }

    /// <summary>
    /// Получает список всех коллайдеров, с которыми пересекается персонаж
    /// </summary>
    public List<Collider2D> GetOverlappingColliders()
    {
        List<Collider2D> result = new List<Collider2D>();

        if (myCollider == null) return result;

        pushOutBuffer.Clear();
        int count = myCollider.Overlap(contactFilter, pushOutBuffer);

        foreach (Collider2D col in pushOutBuffer)
        {
            if (col != null && col != myCollider)
            {
                result.Add(col);
            }
        }

        return result;
    }

    /// <summary>
    /// Принудительное выталкивание из всех коллайдеров
    /// </summary>
    public void ForcePushOut()
    {
        for (int i = 0; i < 5; i++) // Пробуем 5 раз
        {
            PerformPushOut();

            // Проверяем, остались ли пересечения
            if (GetOverlappingColliders().Count == 0)
                break;
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (!showDebug)
            return;

        // Отображение режима коллизий
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Color gizmoColor = collisionMode switch
            {
                CollisionMode.TriggerOnly => triggerColor,
                _ => collisionColor
            };

            Gizmos.color = gizmoColor;

            if (collider is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);

                // Показываем количество объектов внутри
                if (collisionMode == CollisionMode.TriggerOnly && Application.isPlaying)
                {
                    Vector3 labelPos = transform.position + (Vector3)box.offset + Vector3.up * (box.size.y * 0.5f + 0.2f);
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(labelPos, $"Объектов внутри: {currentTriggers.Count}");
#endif
                }
            }
            else if (collider is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            else if (collider is CapsuleCollider2D capsule)
            {
                // Упрощенная визуализация капсулы
                Gizmos.DrawWireSphere(
                    transform.position + (Vector3)capsule.offset + Vector3.up * (capsule.size.y * 0.5f - capsule.size.x * 0.5f),
                    capsule.size.x * 0.5f
                );
                Gizmos.DrawWireSphere(
                    transform.position + (Vector3)capsule.offset - Vector3.up * (capsule.size.y * 0.5f - capsule.size.x * 0.5f),
                    capsule.size.x * 0.5f
                );
            }
        }
    }
}
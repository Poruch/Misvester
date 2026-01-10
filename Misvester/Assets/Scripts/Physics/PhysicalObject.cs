// CollisionPredictor.cs - наследует от CollisionDetector, добавляет физику
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

public class PhysicalObject : CollisionDetector
{

    [Header("Physics Settings")]
    [SerializeField] protected float skinWidth = 0.01f;

    [Header("Push Out Settings")]
    [SerializeField] protected bool isStatic = true;
    [SerializeField] protected bool pushOutEveryFrame = true;
    [SerializeField] protected int maxPushOutAttempts = 3;
    [SerializeField] protected bool pushOutOnStart = true;
    [SerializeField] protected float pushMultiplier = 1.5f; // ← НОВЫЙ ПАРАМЕТР: усиление толчка
    [SerializeField] protected float minPushOutDistance = 0.001f; // 1 мм — игнорировать мелкие пересечения
    [SerializeField] protected float pushOutForce = 0.002f;       // небольшой зазор, чтобы не касаться

    [Header("Physics Debug")]
    [SerializeField] protected Color collisionColor = Color.red;
    [SerializeField] protected bool logPushOut = false;

    // Физические компоненты (те же имена)
    protected Rigidbody2D rb2d;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    [SerializeField] protected float mass = 0;
    protected const int maxCollisionIterations = 3;
    private Vector2 completeReservedPush = Vector2.zero;
    protected override void Awake()
    {
        base.Awake(); // Вызываем Awake родителя

        rb2d = GetComponent<Rigidbody2D>();

        // Инициализация Rigidbody (такая же как в оригинале)
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.simulated = true;
        rb2d.useFullKinematicContacts = true;

    }

    protected override void Start()
    {
        base.Start();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isStatic)
            PerformPushOut();
        ApplyPush();
    }


    /// <summary>
    /// Выталкивание персонажа из всех коллайдеров
    /// </summary>
    public void PerformPushOut()
    {
        if (myCollider == null || rb2d == null) return;

        // Получаем все коллайдеры, с которыми пересекаемся
        if (!UpdateOverlap()) return;

        // Для каждого пересечения пытаемся вытолкнуть персонажа
        foreach (Collider2D otherCollider in overlapBuffer)
        {
            if (otherCollider == null || otherCollider == myCollider)
                continue;
            PushOutOfCollider(otherCollider);
        }
    }

    private void ApplyPush()
    {
        Vector2 newPosition = rb2d.position + completeReservedPush;
        rb2d.MovePosition(newPosition);
        completeReservedPush = Vector2.zero;
    }
    /// <summary>
    /// Пытается вытолкнуть персонажа из конкретного коллайдера
    /// </summary>
    protected void PushOutOfCollider(Collider2D otherCollider)
    {
        if (myCollider == null || otherCollider == null)
            return;

        ColliderDistance2D dist = myCollider.Distance(otherCollider);

        // Нет пересечения или данные недействительны
        if (!dist.isValid || dist.distance >= 0f)
            return;

        // Глубина проникновения (всегда > 0 при пересечении)
        float penetration = -dist.distance;

        // Защита от микропересечений (дрожания)
        if (penetration < minPushOutDistance)
            return;

        // Направление, куда Я должен выйти (от другого объекта)
        Vector2 pushDirection = -dist.normal;

        // Для СТЕН (или если хочешь платформерное поведение) — можно использовать доминантную ось
        // Но для универсальности оставим полную нормаль. Раскомментируй, если нужно:
        if (otherCollider.GetComponent<PhysicalObject>() == null)
        {
            pushDirection = GetDominantAxis(pushDirection);
        }

        if (pushDirection.magnitude < 0.01f)
            return;

        pushDirection.Normalize();

        // Просто выталкиваемся — без учёта масс, без толкания других
        float totalPush = penetration + pushOutForce; // минимальное выталкивание + зазор
        ReceivePush(pushDirection, penetration, totalPush);
    }

    /// <summary>
    /// Пытается толкнуть другой физический объект
    /// </summary>
    /// <param name="other">Объект, которого нужно толкнуть</param>
    /// <param name="pushDirection">Направление толчка (должно быть нормализовано)</param>
    /// <param name="penetration">Глубина проникновения (положительное число)</param>
    public virtual void PushOther(PhysicalObject other, Vector2 pushDirection, float penetration)
    {
        if (other == null || other == this) return;
        if (penetration <= 0f) return; // нет проникновения

        float totalPush = penetration * pushMultiplier + pushOutForce;

        // Масштабируем силу по соотношению масс
        float massRatio = Mathf.Clamp01(mass / (mass + other.mass));
        totalPush *= massRatio;

        other.ReceivePush(pushDirection, penetration, totalPush);
    }

    public virtual void ReceivePush(Vector2 pushDirection, float penetration, float customPushDistance = -1f)
    {
        if (penetration <= 0f) return;

        float totalPush = (customPushDistance < 0f)
            ? (penetration * pushMultiplier + pushOutForce)
            : customPushDistance;

        completeReservedPush += pushDirection * totalPush;
    }




    /// <summary>
    /// Пытается переместить объект на заданное смещение, избегая проникновения в коллайдеры.
    /// Если движение приведёт к коллизии — возвращает укороченный вектор до точки контакта.
    /// </summary>
    /// <param name="move">Желаемое смещение (в мировых координатах)</param>
    /// <returns>Фактическое смещение без проникновения</returns>
    public Vector2 TryMoveWithoutPenetration(Vector2 move)
    {
        if (myCollider == null || rb2d == null || move == Vector2.zero)
            return Vector2.zero;

        // Создаём временный "прогнозируемый" коллайдер (на новой позиции)
        Vector2 originalPosition = rb2d.position;
        Vector2 targetPosition = originalPosition + move;

        // Используем ContactFilter (тот же, что и в CollisionDetector)
        contactFilter.useTriggers = false; // важно: только физические коллайдеры

        // Получаем все коллайдеры, с которыми мы пересечёмся на пути
        RaycastHit2D[] results = new RaycastHit2D[8];
        int hitCount = myCollider.Cast(move, contactFilter, results, move.magnitude);

        if (hitCount == 0)
        {
            // Нет коллизий — можно двигаться полностью
            return move;
        }

        // Находим ближайшее препятствие
        float minDistance = results.Min(x => x.distance);
        // Оставляем небольшой зазор (skin width), чтобы избежать дрожания
        float safeDistance = Mathf.Max(0f, minDistance - skinWidth);
        // Возвращаем укороченное смещение
        return move.normalized * safeDistance;
    }



    public static Vector2 GetDominantAxis(Vector2 vector)
    {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
            return new Vector2(vector.x, 0);
        else
            return new Vector2(0, vector.y);
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
}
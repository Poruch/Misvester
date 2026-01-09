// CollisionPredictor.cs - наследует от CollisionDetector, добавляет физику
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PhysicalObject : CollisionDetector
{
    public enum CollisionMode
    {
        TriggerOnly,    // Только события, без физического взаимодействия
        PushOut,        // Выталкивание из коллайдеров
    }

    [Header("Physics Settings")]
    [SerializeField] protected float skinWidth = 0.01f;
    [SerializeField] protected CollisionMode collisionMode = CollisionMode.PushOut;

    [Header("Push Out Settings")]
    [SerializeField] protected bool pushOutEveryFrame = true;
    [SerializeField] protected float pushOutForce = 0.1f;
    [SerializeField] protected int maxPushOutAttempts = 3;
    [SerializeField] protected bool pushOutOnStart = true;

    [Header("Physics Debug")]
    [SerializeField] protected Color collisionColor = Color.red;
    [SerializeField] protected bool logPushOut = false;

    // Физические компоненты (те же имена)
    protected Rigidbody2D rb2d;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    [SerializeField] protected float mass = 0;
    protected const int maxCollisionIterations = 3;
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
        PerformPushOut();
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

    [SerializeField] protected float pushMultiplier = 1.2f; // ← НОВЫЙ ПАРАМЕТР: усиление толчка

    /// <summary>
    /// Пытается вытолкнуть персонажа из конкретного коллайдера
    /// </summary>
    protected void PushOutOfCollider(Collider2D otherCollider)
    {
        if (myCollider == null || otherCollider == null) return;
        if (collisionMode != CollisionMode.PushOut) return;

        ColliderDistance2D dist = myCollider.Distance(otherCollider);
        if (!dist.isValid || dist.distance >= 0f) return;
        float penetration = -dist.distance;

        Vector2 pushDir = GetDominantAxis(-dist.normal);
        if (pushDir.magnitude < 0.01f) return;
        pushDir.Normalize();

        PhysicalObject other = otherCollider.GetComponent<PhysicalObject>();

        if (other == null)
        {
            // Стена — просто выталкиваемся с усиленной силой
            ReceivePush(pushDir, penetration);
            return;
        }

        if (mass > other.mass)
        {
            PushOther(other, pushDir, penetration);
        }
        else if (mass < other.mass)
        {
            // Легче — слабо отталкиваемся
            ReceivePush(pushDir, penetration, pushOutForce * 0.5f);
        }
        else
        {
            // Равные массы
            float basePush = -dist.distance;
            float totalPush = basePush * pushMultiplier + pushOutForce;
            Vector2 halfPush = pushDir * (totalPush * 0.5f);

            other.rb2d.MovePosition(other.rb2d.position + halfPush);
            rb2d.MovePosition(rb2d.position - halfPush);
        }
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

        Vector2 newPosition = rb2d.position + pushDirection * totalPush;
        rb2d.MovePosition(newPosition);
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

    /// <summary>
    /// Включение/выключение выталкивания
    /// </summary>
    public void EnablePushOut(bool enable)
    {
        collisionMode = enable ? CollisionMode.PushOut : CollisionMode.TriggerOnly;
    }
}
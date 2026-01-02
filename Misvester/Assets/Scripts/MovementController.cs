using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс для управления движением объектов с различными режимами
/// </summary>
public class MovementController : CollisionPredictor
{
    public enum MovementType
    {
        Directional,    // Движение по заданному направлению
        Target,         // Движение к цели
        Path,           // Движение по пути
        Random,         // Случайное движение
        Follow,         // Следование за объектом
        Oscillate,      // Колебательное движение
        Homing          // Самонаведение на цель
    }

    public enum UpdateMode
    {
        FixedUpdate,    // В FixedUpdate (для физики)
        Update,         // В Update (для плавности)
        Manual          // Ручное управление
    }

    [Header("Movement Settings")]
    [SerializeField] private MovementType movementType = MovementType.Directional;
    [SerializeField] private UpdateMode updateMode = UpdateMode.FixedUpdate;
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private bool useSmoothMovement = true;
    [SerializeField] private bool rotateToDirection = true;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float targetReachedDistance = 0.1f;
    [SerializeField] private bool stopOnTargetReached = true;

    [Header("Path Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loopPath = true;
    [SerializeField] private float waypointReachedDistance = 0.1f;

    [Header("Random Movement")]
    [SerializeField] private float randomDirectionChangeTime = 2f;
    [SerializeField] private float randomSpeedVariation = 2f;

    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float followSmoothness = 5f;

    [Header("Oscillation Settings")]
    [SerializeField] private float oscillationAmplitude = 2f;
    [SerializeField] private float oscillationFrequency = 1f;
    [SerializeField] private Vector2 oscillationAxis = Vector2.right;

    [Header("Homing Settings")]
    [SerializeField] private float homingTurnSpeed = 90f;
    [SerializeField] private float homingRange = 10f;

    [Header("Events")]
    public UnityEvent onTargetReached = new UnityEvent();
    public UnityEvent onWaypointReached = new UnityEvent();
    public UnityEvent onMovementStarted = new UnityEvent();
    public UnityEvent onMovementStopped = new UnityEvent();

    // Runtime variables
    private Vector2 currentVelocity;
    private Vector2 desiredDirection;
    private float currentSpeed;
    private int currentWaypointIndex = 0;
    private float randomTimer;
    private Vector2 randomDirection;
    private Vector2 oscillationStartPosition;
    private float oscillationTimer;
    private bool isMoving = false;

    protected override void Awake()
    {
        base.Awake();
        oscillationStartPosition = rb2d.position;
        randomDirection = GetRandomDirection();
        currentSpeed = speed;
    }

    protected override void Update()
    {
        base.Update();
        if (updateMode == UpdateMode.Update)
        {
            float deltaTime = Time.deltaTime;
            ProcessMovement(deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate)
        {
            float deltaTime = Time.fixedDeltaTime;
            ProcessMovement(deltaTime);
        }
    }
    /// <summary>
    /// Безопасное перемещение с обработкой коллизий
    /// </summary>
    public virtual void Move(Vector2 movement)
    {
        if (collisionMode == CollisionMode.TriggerOnly)
        {
            // Только триггерная проверка
            CheckTriggerMovement(movement);
            rb2d.position += movement;
            return;
        }

        // Полноценная обработка коллизий
        Vector2 remainingMovement = movement;

        // Итеративная обработка коллизий
        for (int i = 0; i < maxCollisionIterations; i++)
        {
            if (remainingMovement.magnitude < 0.001f)
                break;

            if (!CheckAndResolveCollision(ref remainingMovement))
                break;
        }

        // Применяем оставшееся движение
        rb2d.position += remainingMovement;

        // Визуализация
        if (showDebug)
            DebugDrawMovement(movement, remainingMovement);
    }

    /// <summary>
    /// Основной метод обработки движения
    /// </summary>
    private void ProcessMovement(float deltaTime)
    {
        if (!isMoving)
            return;

        // Получаем желаемое направление и скорость в зависимости от типа движения
        Vector2 targetVelocity = GetTargetVelocity(deltaTime);

        // Плавное изменение скорости
        if (useSmoothMovement)
        {
            float targetSpeed = targetVelocity.magnitude;
            Vector2 targetDir = targetVelocity.normalized;

            // Плавное изменение скорости
            if (targetSpeed > currentSpeed)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * deltaTime);
            }

            // Плавное изменение направления
            if (desiredDirection != Vector2.zero && targetDir != Vector2.zero)
            {
                desiredDirection = Vector2.MoveTowards(desiredDirection, targetDir, rotationSpeed * deltaTime);
            }
            else
            {
                desiredDirection = targetDir;
            }

            currentVelocity = desiredDirection * currentSpeed;
        }
        else
        {
            currentVelocity = targetVelocity;
            currentSpeed = currentVelocity.magnitude;
        }

        // Применяем движение с обработкой коллизий
        if (currentVelocity.magnitude > 0.001f)
        {
            Vector2 movement = currentVelocity * deltaTime;
            Move(movement);

            // Поворот в направлении движения
            if (rotateToDirection && currentVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }
        else if (isMoving)
        {
            StopMovement();
        }
    }

    /// <summary>
    /// Получение целевой скорости в зависимости от типа движения
    /// </summary>
    private Vector2 GetTargetVelocity(float deltaTime)
    {
        switch (movementType)
        {
            case MovementType.Directional:
                return direction.normalized * speed;

            case MovementType.Target:
                return GetTargetVelocity();

            case MovementType.Path:
                return GetPathVelocity();

            case MovementType.Random:
                return GetRandomVelocity(deltaTime);

            case MovementType.Follow:
                return GetFollowVelocity();

            case MovementType.Oscillate:
                return GetOscillationVelocity(deltaTime);

            case MovementType.Homing:
                return GetHomingVelocity(deltaTime);

            default:
                return Vector2.zero;
        }
    }

    /// <summary>
    /// Движение к цели
    /// </summary>
    private Vector2 GetTargetVelocity()
    {
        if (target == null)
            return Vector2.zero;

        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        float distance = toTarget.magnitude;

        if (distance < targetReachedDistance)
        {
            if (stopOnTargetReached)
            {
                onTargetReached?.Invoke();
                return Vector2.zero;
            }
        }

        return toTarget.normalized * speed;
    }

    /// <summary>
    /// Движение по пути
    /// </summary>
    private Vector2 GetPathVelocity()
    {
        if (waypoints == null || waypoints.Length == 0)
            return Vector2.zero;

        Transform currentWaypoint = waypoints[currentWaypointIndex];
        Vector2 toWaypoint = (Vector2)currentWaypoint.position - rb2d.position;
        float distance = toWaypoint.magnitude;

        if (distance < waypointReachedDistance)
        {
            currentWaypointIndex++;
            onWaypointReached?.Invoke();

            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopPath)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    StopMovement();
                    return Vector2.zero;
                }
            }
        }

        return toWaypoint.normalized * speed;
    }

    /// <summary>
    /// Случайное движение
    /// </summary>
    private Vector2 GetRandomVelocity(float deltaTime)
    {
        randomTimer += deltaTime;

        if (randomTimer >= randomDirectionChangeTime)
        {
            randomDirection = GetRandomDirection();
            currentSpeed = speed + Random.Range(-randomSpeedVariation, randomSpeedVariation);
            randomTimer = 0f;
        }

        return randomDirection * currentSpeed;
    }

    /// <summary>
    /// Следование за объектом
    /// </summary>
    private Vector2 GetFollowVelocity()
    {
        if (target == null)
            return Vector2.zero;

        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        float distance = toTarget.magnitude;

        if (distance <= followDistance)
            return Vector2.zero;

        Vector2 desiredPosition = (Vector2)target.position - toTarget.normalized * followDistance;
        Vector2 direction = (desiredPosition - rb2d.position).normalized;

        return direction * speed;
    }

    /// <summary>
    /// Колебательное движение
    /// </summary>
    private Vector2 GetOscillationVelocity(float deltaTime)
    {
        oscillationTimer += deltaTime * oscillationFrequency * Mathf.PI * 2f;

        // Позиция на синусоиде
        Vector2 offset = oscillationAxis * Mathf.Sin(oscillationTimer) * oscillationAmplitude;
        Vector2 targetPosition = oscillationStartPosition + offset;

        // Направление к следующей позиции
        Vector2 direction = (targetPosition - rb2d.position).normalized;

        return direction * speed;
    }

    /// <summary>
    /// Самонаведение на цель
    /// </summary>
    private Vector2 GetHomingVelocity(float deltaTime)
    {
        if (target == null)
            return direction.normalized * speed;

        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        float distance = toTarget.magnitude;

        if (distance > homingRange)
            return direction.normalized * speed;

        // Плавный поворот к цели
        Vector2 currentDir = direction.normalized;
        Vector2 targetDir = toTarget.normalized;

        float maxAngleChange = homingTurnSpeed * deltaTime;
        float angle = Vector2.SignedAngle(currentDir, targetDir);
        float angleChange = Mathf.Clamp(angle, -maxAngleChange, maxAngleChange);

        direction = Quaternion.Euler(0, 0, angleChange) * currentDir;

        return direction.normalized * speed;
    }

    /// <summary>
    /// Получение случайного направления
    /// </summary>
    private Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// Публичные методы управления
    /// </summary>

    /// <summary>
    /// Начать движение
    /// </summary>
    public void StartMovement()
    {
        isMoving = true;
        onMovementStarted?.Invoke();
    }

    /// <summary>
    /// Остановить движение
    /// </summary>
    public void StopMovement()
    {
        isMoving = false;
        currentVelocity = Vector2.zero;
        currentSpeed = 0f;
        onMovementStopped?.Invoke();
    }

    /// <summary>
    /// Установить направление движения
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (!isMoving) StartMovement();
    }

    /// <summary>
    /// Установить скорость
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        currentSpeed = newSpeed;
    }

    /// <summary>
    /// Установить цель
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (movementType == MovementType.Target && !isMoving)
            StartMovement();
    }

    /// <summary>
    /// Установить тип движения
    /// </summary>
    public void SetMovementType(MovementType type)
    {
        movementType = type;
        InitializeMovementType();
    }

    /// <summary>
    /// Инициализация типа движения
    /// </summary>
    private void InitializeMovementType()
    {
        switch (movementType)
        {
            case MovementType.Oscillate:
                oscillationStartPosition = rb2d.position;
                oscillationTimer = 0f;
                break;

            case MovementType.Random:
                randomDirection = GetRandomDirection();
                randomTimer = 0f;
                break;

            case MovementType.Path:
                currentWaypointIndex = 0;
                break;
        }
    }

    /// <summary>
    /// Установить путь
    /// </summary>
    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypointIndex = 0;
        if (movementType == MovementType.Path && !isMoving)
            StartMovement();
    }

    /// <summary>
    /// Добавить относительное движение (игнорируя текущий режим)
    /// </summary>
    public void AddRelativeMovement(Vector2 movement)
    {
        Move(movement);
    }

    /// <summary>
    /// Телепортировать в позицию
    /// </summary>
    public void Teleport(Vector2 position)
    {
        rb2d.position = position;
        oscillationStartPosition = position;
    }

    /// <summary>
    /// Получить текущую скорость
    /// </summary>
    public Vector2 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    /// <summary>
    /// Получить текущее направление
    /// </summary>
    public Vector2 GetCurrentDirection()
    {
        return currentVelocity.normalized;
    }

    /// <summary>
    /// Проверить, движется ли объект
    /// </summary>
    public bool IsMoving()
    {
        return isMoving && currentVelocity.magnitude > 0.01f;
    }

    /// <summary>
    /// Перезапустить движение с начальной позиции
    /// </summary>
    public void ResetMovement()
    {
        Teleport(oscillationStartPosition);
        currentWaypointIndex = 0;
        randomTimer = 0f;
        oscillationTimer = 0f;
        currentVelocity = Vector2.zero;
        currentSpeed = speed;
        isMoving = false;
    }


    /// <summary>
    /// Визуализация
    /// </summary>
    private void DebugDrawMovement(Vector2 originalMovement, Vector2 finalMovement)
    {
        if (!showDebug)
            return;

        Vector2 startPos = rb2d.position - originalMovement;

        // Цвет в зависимости от режима
        Color modeColor = collisionMode == CollisionMode.TriggerOnly ?
            triggerColor : collisionColor;

        // Оригинальный путь
        Debug.DrawLine(startPos, startPos + originalMovement, modeColor, 1f);

        // Фактический путь
        Debug.DrawLine(startPos, rb2d.position, Color.green, 1f);

        // Нормали столкновений
        for (int i = 0; i < hitBuffer.Length; i++)
        {
            if (hitBuffer[i].collider != null)
            {
                Debug.DrawRay(
                    hitBuffer[i].point,
                    hitBuffer[i].normal,
                    Color.blue,
                    1f
                );
            }
        }
    }
    // Для отладки
    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        // Направление движения
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, direction * 2f);

        // Путь
        if (movementType == MovementType.Path && waypoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
                if (loopPath && i == waypoints.Length - 1 && waypoints[0] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }

        // Радиус самонаведения
        if (movementType == MovementType.Homing)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, homingRange);
        }

        // Дистанция следования
        if (movementType == MovementType.Follow)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, followDistance);
        }
    }
}
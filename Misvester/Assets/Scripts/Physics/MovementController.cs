using UnityEngine;
using UnityEngine.Events;

public class MovementController : PhysicalObject
{
    public enum MovementType
    {
        Directional, Target, Path, Random, Follow, Oscillate, Homing
    }

    [Header("Movement")]
    [SerializeField] private MovementType movementType = MovementType.Directional;
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 15f;   // плавный разгон

    [Header("Target / Path")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointDistance = 0.2f;
    [SerializeField] private bool loopPath = true;

    [Header("Random")]
    [SerializeField] private float changeDirInterval = 2f;

    [Header("Follow")]
    [SerializeField] private float followDistance = 2f;

    [Header("Oscillate")]
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private Vector2 axis = Vector2.right;

    [Header("Homing")]
    [SerializeField] private float homingRange = 10f;
    [SerializeField] private float turnSpeed = 180f;

    [Header("Events")]
    public UnityEvent onTargetReached;

    private Vector2 targetVelocity;
    private int currentWaypoint;                   
    private float randomTimer;
    private Vector2 randomDir;
    private Vector2 startPos;
    private float oscTime;

    protected override void Awake()
    {
        base.Awake();
        startPos = rb.position;
        randomDir = Random.insideUnitCircle.normalized;
    }

    private void FixedUpdate()
    {
        if (isStatic) return;

        Vector2 desiredVelocity = GetDesiredVelocity();
        // Плавное изменение скорости
        Vector2 newPos = rb.position + desiredVelocity * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        //// Поворот в направлении движения (опционально)
        //if (targetVelocity.magnitude > 0.1f && movementType != MovementType.Oscillate)
        //{
        //    float angle = Mathf.Atan2(targetVelocity.y, targetVelocity.x) * Mathf.Rad2Deg;
        //    rb.rotation = angle;
        //}
    }

    private Vector2 GetDesiredVelocity()
    {
        Vector2 vel = Vector2.zero;
        switch (movementType)
        {
            case MovementType.Directional:
                vel = direction.normalized * speed;
                break;

            case MovementType.Target:
                if (target != null)
                {
                    Vector2 toTarget = (Vector2)target.position - rb.position;
                    if (toTarget.magnitude < 0.1f)
                        onTargetReached?.Invoke();
                    else
                        vel = toTarget.normalized * speed;
                }
                break;

            case MovementType.Path:
                if (waypoints.Length > 0)
                {
                    Vector2 toWaypoint = (Vector2)waypoints[currentWaypoint].position - rb.position;
                    if (toWaypoint.magnitude < waypointDistance)
                    {
                        currentWaypoint++;
                        if (currentWaypoint >= waypoints.Length)
                            currentWaypoint = loopPath ? 0 : waypoints.Length - 1;
                    }
                    vel = toWaypoint.normalized * speed;
                }
                break;

            case MovementType.Random:
                randomTimer += Time.fixedDeltaTime;
                if (randomTimer >= changeDirInterval)
                {
                    randomDir = Random.insideUnitCircle.normalized;
                    randomTimer = 0f;
                }
                vel = randomDir * speed;
                break;

            case MovementType.Follow:
                if (target != null)
                {
                    Vector2 toTarget = (Vector2)target.position - rb.position;
                    if (toTarget.magnitude > followDistance)
                        vel = toTarget.normalized * speed;
                }
                break;

            case MovementType.Oscillate:
                oscTime += Time.fixedDeltaTime * frequency * Mathf.PI * 2f;
                Vector2 offset = axis * Mathf.Sin(oscTime) * amplitude;
                Vector2 desiredPos = startPos + offset;
                vel = (desiredPos - rb.position) * speed; // скорость, направленная к цели
                break;

            case MovementType.Homing:
                if (target != null)
                {
                    Vector2 toTarget = (Vector2)target.position - rb.position;
                    if (toTarget.magnitude < homingRange)
                    {
                        Vector2 currentDir = targetVelocity.normalized;
                        if (currentDir == Vector2.zero) currentDir = direction.normalized;
                        Vector2 newDir = currentDir.RotateTowards(toTarget.normalized, turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 1f);
                        vel = newDir * speed;
                    }
                    else vel = direction.normalized * speed;
                }
                else vel = direction.normalized * speed;
                break;
        }
        return vel;
    }
    [Header("Collision Response")]
    [SerializeField] private LayerMask wallLayers; // выберите слой стен в инспекторе

    // Публичные методы управления
    public void SetDirection(Vector2 dir) => direction = dir.normalized;
    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetTarget(Transform t) => target = t;
    public void Stop() => rb.linearVelocity = Vector2.zero;
    public void Teleport(Vector2 pos) => rb.position = pos;
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PhysicalObject : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] protected float mass = 1f;
    [SerializeField] protected bool isStatic = false; // для объектов, которые не должны двигаться от сил

    protected Rigidbody2D rb;
    protected Collider2D col;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (!isStatic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;          // top-down: нет гравитации
            rb.linearDamping = 3f;          // естественное торможение
            rb.angularDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    // Опционально: метод для получения силы толчка (если нужно извне)
    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
    {
        if (!isStatic) rb.AddForce(force, mode);
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (!isStatic) rb.linearVelocity = velocity;
    }
}
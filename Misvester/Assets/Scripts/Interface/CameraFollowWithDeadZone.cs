using UnityEngine;

public class CameraFollowWithDeadZone : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // Сюда перетащи объект игрока в инспекторе

    [Header("Dead Zone (in world units)")]
    public Vector2 deadZoneSize = new Vector2(4f, 3f); // Ширина и высота "тихой" зоны

    [Header("Smoothing")]
    [Range(0f, 1f)] public float smoothFactor = 0.1f; // 0 = мгновенно, 1 = очень плавно

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null) return;

        // Текущая позиция камеры и целевая позиция (позиция игрока)
        Vector3 targetPosition = player.position;

        // Ограничения по X: камера двигается, только если игрок вышел за пределы dead zone
        float cameraX = transform.position.x;
        float playerX = player.position.x;

        float leftLimit = cameraX - deadZoneSize.x / 2f;
        float rightLimit = cameraX + deadZoneSize.x / 2f;

        if (playerX < leftLimit)
            cameraX = playerX + deadZoneSize.x / 2f;
        else if (playerX > rightLimit)
            cameraX = playerX - deadZoneSize.x / 2f;

        // То же самое по Y (если нужно вертикальное следование)
        float cameraY = transform.position.y;
        float playerY = player.position.y;

        float bottomLimit = cameraY - deadZoneSize.y / 2f;
        float topLimit = cameraY + deadZoneSize.y / 2f;

        if (playerY < bottomLimit)
            cameraY = playerY + deadZoneSize.y / 2f;
        else if (playerY > topLimit)
            cameraY = playerY - deadZoneSize.y / 2f;

        // Формируем желаемую позицию камеры
        Vector3 desiredPosition = new Vector3(cameraX, cameraY, transform.position.z);

        // Плавное перемещение
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothFactor
        );

        transform.position = smoothedPosition;
    }
}
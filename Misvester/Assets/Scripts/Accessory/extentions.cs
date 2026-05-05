using UnityEngine;

public static class Vector2Extensions
{
    /// <summary>
    /// Поворачивает вектор на заданный угол (в градусах) против часовой стрелки.
    /// </summary>
    /// <param name="v">Исходный вектор</param>
    /// <param name="angleDeg">Угол поворота в градусах</param>
    /// <returns>Повёрнутый вектор</returns>
    public static Vector2 Rotate(this Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    public static Vector2 RotateTowards(this Vector2 current, Vector2 target, float maxRadiansDelta, float maxMagnitudeDelta)
    {
        // Вычисляем угол между векторами
        float angleCurrent = Mathf.Atan2(current.y, current.x);
        float angleTarget = Mathf.Atan2(target.y, target.x);

        // Разница углов
        float delta = Mathf.DeltaAngle(angleCurrent * Mathf.Rad2Deg, angleTarget * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        // Ограничиваем изменение
        float newDelta = Mathf.Clamp(delta, -maxRadiansDelta, maxRadiansDelta);
        float newAngle = angleCurrent + newDelta;

        // Возвращаем новый нормализованный вектор
        return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
    }
}
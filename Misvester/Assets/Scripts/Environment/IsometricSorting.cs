using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]
public class IsometricSortecLevel : MonoBehaviour
{
    [Tooltip("„ем больше значение, тем сильнее вли€ет Y на пор€док")]
    public float depthMultiplier = 100f; // подбери опытным путЄм

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // „ем ниже Y Ч тем больше sortingOrder (ближе к камере)
        // Unity: больший sortingOrder = рисуетс€ ѕќ¬≈–’
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * depthMultiplier);
    }
}

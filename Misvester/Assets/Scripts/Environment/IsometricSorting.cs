using UnityEngine;

public class IsometricSortLevel : MonoBehaviour
{
    [Tooltip("„ем больше значение, тем сильнее вли€ет Y на пор€док")]
    public float depthMultiplier = 100f;
    [SerializeField] public int offset = 0;
    [SerializeField] private SpriteRenderer[] spriteRenderers = new SpriteRenderer[1];

    void Awake()
    {
        if (!spriteRenderers[0])
            spriteRenderers[0] = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // „ем ниже Y Ч тем больше sortingOrder (ближе к камере)
        // Unity: больший sortingOrder = рисуетс€ ѕќ¬≈–’
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingOrder = offset + -i + Mathf.RoundToInt(-transform.position.y * depthMultiplier);
        }
    }
}

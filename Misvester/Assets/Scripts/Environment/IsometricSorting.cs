using UnityEngine;

public class IsometricSortLevel : MonoBehaviour
{
    public float depthMultiplier = 10000f;
    [SerializeField] public int offset = 0;
    [SerializeField] private SpriteRenderer[] spriteRenderers = new SpriteRenderer[1];
    [SerializeField] Transform obj;

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
            if (!obj)
                spriteRenderers[i].sortingOrder = offset + -i + Mathf.RoundToInt(-transform.position.y * depthMultiplier);
            else
                spriteRenderers[i].sortingOrder = offset + -i + Mathf.RoundToInt(-obj.position.y * depthMultiplier);
        }
    }
}

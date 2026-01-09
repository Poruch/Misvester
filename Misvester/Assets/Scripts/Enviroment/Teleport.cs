using UnityEngine;

public class Teleport : CollisionDetector
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Teleport exit;
    [SerializeField] Vector2 offset = Vector2.zero;
    void Start()
    {
        onTriggerEnter.AddListener(onTeleport);
    }

    public void onTeleport(GameObject gameObject)
    {
        if (exit)
            gameObject.transform.position = (Vector2)exit.transform.position + exit.offset;
        else
            gameObject.transform.position = (Vector2)transform.position + offset;
    }
}

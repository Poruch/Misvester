using Assets.Scripts.NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class DetectionArea : MonoBehaviour
{
    [SerializeField] UnityEvent<GameObject> onColliderEnter = new();
    [SerializeField] UnityEvent<GameObject> onColliderExit = new();
    [SerializeField] CollisionDetector predictor = null;
    private void Awake()
    {
        predictor.onTriggerEnter.AddListener(OnCEnter);
        predictor.onTriggerExit.AddListener(OnCExit);
    }

    public T GetFirstByType<T>() where T : Component
    {
        var objects = predictor.GetComponentsInside<T>();
        if (objects.Count > 0)
            return objects[0];
        return null;
    }
    public void SetPosition(Vector2 newPosition)
    {
        transform.localPosition = newPosition;
    }

    public void SetCollisionMask()
    {

    }


    public UnityEvent<GameObject> OnColliderEnter { get => onColliderEnter; set => onColliderEnter = value; }
    public UnityEvent<GameObject> OnColliderExit { get => onColliderExit; set => onColliderExit = value; }

    private void OnCEnter(GameObject gameObject)
    {
        OnColliderEnter.Invoke(gameObject);
    }
    private void OnCExit(GameObject gameObject)
    {
        OnColliderExit.Invoke(gameObject);
    }
}

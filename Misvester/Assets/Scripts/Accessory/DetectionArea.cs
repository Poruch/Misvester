using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class DetectionArea : MonoBehaviour
{
    [SerializeField] UnityEvent<GameObject> onColliderEnter  = new();
    [SerializeField] CollisionPredictor predictor = null;
    private void Awake()
    {
        predictor.onTriggerEnter.AddListener(OnCollision);
    }
    public void SetPosition(Vector2 newPosition)
    {
        transform.localPosition = newPosition;
    }

    public void SetCollisionMask()
    {

    }


    public UnityEvent<GameObject> OnColliderEnter { get => onColliderEnter; set => onColliderEnter = value; }


    private void OnCollision(GameObject gameObject)
    { 
        OnColliderEnter.Invoke(gameObject);
    }
}

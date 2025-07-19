using UnityEngine;

[CreateAssetMenu(fileName = "BonusConfig", menuName = "Scriptable Objects/BonusConfig")]
public class BonusConfig : ScriptableObject
{
    [SerializeField]
    GameObject[] bonuses;

    public GameObject[] Bonuses { get => bonuses; set => bonuses = value; }
}


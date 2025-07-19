using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [SerializeField]
    EnemyConfig[] enemies;
    [SerializeField]
    bool isEndless = false;
    [SerializeField]
    BonusConfig bonusConfig;
    public EnemyConfig[] Enemies { get => enemies; set => enemies = value; }
    public BonusConfig BonusConfig { get => bonusConfig; set => bonusConfig = value; }
    public bool IsEndless { get => isEndless; set => isEndless = value; }
}


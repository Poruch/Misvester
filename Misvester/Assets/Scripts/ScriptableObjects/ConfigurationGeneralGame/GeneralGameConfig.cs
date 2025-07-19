using UnityEngine;

[CreateAssetMenu(fileName = "GeneralGameConfig", menuName = "Scriptable Objects/GeneralGameConfig")]
public class GeneralGameConfig : ScriptableObject
{
    [SerializeField]
    PlayerConfig playerConfig;
    [SerializeField]
    LevelConfig levelConfig;
    [SerializeField]
    Vector2 startPosition;
    [SerializeField]
    int dificultLevel;
    public PlayerConfig PlayerConfig { get => playerConfig; set => playerConfig = value; }
    public Vector2 StartPosition { get => startPosition; set => startPosition = value; }
    public LevelConfig LevelConfig { get => levelConfig; set => levelConfig = value; }
    public int DificultLevel { get => dificultLevel; set => dificultLevel = value; }
}

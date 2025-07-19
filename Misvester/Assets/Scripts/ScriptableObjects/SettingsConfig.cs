using UnityEngine;

[CreateAssetMenu(fileName = "SettingsConfig", menuName = "Scriptable Objects/SettingsConfig")]
public class SettingsConfig : ScriptableObject
{
    [SerializeField] int fps;
    public int Fps { get => fps; set => fps = value; }
}

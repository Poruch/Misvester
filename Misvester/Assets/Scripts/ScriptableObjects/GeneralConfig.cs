using UnityEngine;

[CreateAssetMenu(fileName = "GeneralConfig", menuName = "Scriptable Objects/GeneralConfig")]
public class GeneralConfig : ScriptableObject
{

    [SerializeField] SettingsConfig settingsConfig;
    [SerializeField] LanguageConfig languageConfig;
   

    public LanguageConfig LanguageConfig { get => languageConfig; set => languageConfig = value; }
    public SettingsConfig SettingsConfig { get => settingsConfig; set => settingsConfig = value; }
}

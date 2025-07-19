
using TMPro;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsInput;


    //[SerializeField] GeneralGameConfig config;

    public void Init()
    {

    }

    public void Accept()
    {
        int fps = 60;
        if (!int.TryParse(fpsInput.text.Substring(0, fpsInput.text.Length - 1), out fps))
            Debug.LogWarning("Неправильный формат ввода кадров в секунду " + fpsInput.text);
        //SettingManager.Instance.SetFrameRate(fps);
    }


    public void Easy()
    {
        FloatingTextManager.Instance.CreateFloatingText("Dificult easy", transform.position + Vector3.up, Color.green);
        //config.DificultLevel = 0;
    }
    public void Medium()
    {
        FloatingTextManager.Instance.CreateFloatingText("Dificult medium", transform.position, Color.blue);
        //config.DificultLevel = 5;
    }
    public void Hard()
    {
        FloatingTextManager.Instance.CreateFloatingText("Dificult hard", transform.position + Vector3.down, Color.red);
        //config.DificultLevel = 10;
    }
}

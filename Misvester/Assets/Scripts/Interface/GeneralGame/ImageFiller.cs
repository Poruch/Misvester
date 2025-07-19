using Assets.Scripts.Accessory;
using Assets.Scripts.GeneralGame;
using System;
using UnityEngine;
using UnityEngine.UI;
public class ImageFiller : MonoBehaviour
{
    Image image;
    void Start()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    Func<int> value;
    Timer timer = TimeManager.Instance.CreateTimer(1);
    public void Instantiate(Func<int> value, Timer timer)
    {
        this.value = value;
        this.timer = timer;
    }
    public void OnValueChange(float value)
    {
        if (image == null)
            image = GetComponent<Image>();
        image.fillAmount = value;
    }


    // Update is called once per frame
    void Update()
    {
        if (value != null && timer.IsTime)
        {
            image.fillAmount = value.Invoke();
        }
    }
}

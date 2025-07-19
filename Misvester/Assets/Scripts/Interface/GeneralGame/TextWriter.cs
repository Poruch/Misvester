using Assets.Scripts.Accessory;
using Assets.Scripts.GeneralGame;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Interface.GeneralGame
{
    public class TextWriter : MonoBehaviour
    {
        Func<TextWriter, string> text;
        TextMeshProUGUI textMeshProUGUI;
        Timer timer = TimeManager.Instance.CreateTimer(1, true);
        public TextMeshProUGUI TextMeshProUGUI { get => textMeshProUGUI; set => textMeshProUGUI = value; }
        bool isDisable = false;
        public void Disable()
        {
            textMeshProUGUI.text = "";
            isDisable = true;
            timer.IsStopped = true;
            timer.Reset();
        }
        public void Enable()
        {
            timer.IsStopped = false;
            isDisable = false;
        }
        private void Start()
        {
            TextMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }
        public void Instantiate(Func<TextWriter, string> text, Timer timer)
        {
            this.text = text;
            this.timer = timer;
        }
        private void Update()
        {
            if (timer.IsTime && !isDisable)
                TextMeshProUGUI.text = text.Invoke(this);
        }

    }
}

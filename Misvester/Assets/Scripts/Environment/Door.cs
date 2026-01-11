using Assets.Scripts.Accessory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    internal class Door : Interactive
    {
        Collider2D collider = null;
        Timer timer = TimeManager.Instance.CreateTimer(4);
        SpriteRenderer spriteRenderer = null;
        Color baseColor = Color.white;
        private void Start()
        {
            collider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;
            Close();
        }
        public override bool Interact(InteractArgs args)
        {
            if (!collider || !base.Interact(args)) { return false; }

            if (collider.enabled)
            {
                Open();
            }
            else
            {
                Close();
            }
            return true;
        }
        public void Open()
        {
            collider.enabled = false;
            spriteRenderer.color = new Color(0, 100, 0);
        }
        public void Close()
        {
            collider.enabled = true;
            spriteRenderer.color = baseColor;
        }
        private void Update()
        {
            if (collider)
            {
                if (timer.IsTime)
                {
                    //collider.enabled = true;
                }
            }
        }

    }
}

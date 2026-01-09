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
        private void Start()
        {
            collider = GetComponent<Collider2D>();
        }
        public override bool Interact(InteractArgs args)
        {
            if (!collider || !base.Interact(args)) { return false; }

            collider.enabled = !collider.enabled;
            return true;
        }
        private void Update()
        {
            if (collider)
            {
                if (timer.IsTime)
                {
                    collider.enabled = true;
                }
            }
        }

    }
}

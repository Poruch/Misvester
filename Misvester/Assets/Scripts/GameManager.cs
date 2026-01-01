using Assets.Scripts.Accessory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class GameManager : MonoBehaviour
    {




        private void Update()
        {
            TimeManager.Instance.Update();
        }
    }
}

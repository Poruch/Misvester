using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.NPC
{
    public class Interactive : MonoBehaviour
    {
        [SerializeField] bool isTrueCanInteracted = true;
        [SerializeField] bool canInteracted = false;
        [SerializeField] GameObject hint;
        protected virtual void onSetInteracted(bool value)
        {
            if (hint)
            {
                hint.SetActive(value);
            }
        }
        public bool CanInteracted
        {
            set
            {
                canInteracted = value;
                onSetInteracted(value);
            }
            get
            {
                return canInteracted;
            }
        }

        public virtual bool Interact(InteractArgs args)
        {
            if (!canInteracted || !isTrueCanInteracted) return false;
            Debug.Log("Player interact  -" + gameObject.name);
            return true;
        }



    }
}

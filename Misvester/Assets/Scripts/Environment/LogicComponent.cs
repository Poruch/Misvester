
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Environment
{
    internal class LogicComponent : MonoBehaviour
    {
        bool isEnable = false;
        public bool IsEnable
        {
            set
            {
                isEnable = value;
                ChangeImpulse?.Invoke(isEnable);
            }
            get
            {
                return isEnable;
            }
        }

        public UnityEvent<bool> ChangeImpulse { get => changeImpulse; set => changeImpulse = value; }

        UnityEvent<bool> changeImpulse = new UnityEvent<bool>();

        protected virtual bool ActivationFunc()
        {
            return false;
        }
        protected virtual void Start()
        {

        }
    }
}

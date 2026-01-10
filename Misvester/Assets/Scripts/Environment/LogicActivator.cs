using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Environment
{
    internal class LogicActivator : LogicComponent
    {
        [SerializeField] private List<LogicComponent> components = new List<LogicComponent>();

        // События
        [SerializeField] private UnityEvent onAllActivated;      // Все компоненты активны
        //[SerializeField] private UnityEvent onAnyActivated;      // Хотя бы один активен (состояние активации)
        [SerializeField] private UnityEvent onAllDeactivated;    // Все компоненты неактивны (выход из активации)

        private bool wasAllActivated = false;

        protected override void Start()
        {
            base.Start();
            for (int i = 0; i < components.Count; i++)
            {
                components[i].ChangeImpulse.AddListener(OnImpulseChange);
            }
        }

        private void Update()
        {

        }
        protected void OnImpulseChange(bool value)
        {
            CheckState();
        }
        private void CheckState()
        {
            int activeCount = 0;
            foreach (var comp in components)
            {
                if (comp != null && comp.IsEnable)
                    activeCount++;
            }

            bool allActive = activeCount == components.Count && components.Count > 0;
            bool allInactive = activeCount == 0 && components.Count > 0;

            // Событие: все активированы (только при переходе в это состояние)
            if (allActive && !wasAllActivated)
            {
                onAllActivated?.Invoke();
            }
            wasAllActivated = allActive;

            // Событие: выход из активации (все неактивны)
            if (allInactive)
            {
                onAllDeactivated?.Invoke();
            }
        }

        // Опционально: если нужно принудительно проверить состояние вручную
        public void ForceCheck()
        {
            CheckState();
        }

        // Активация по умолчанию — зависит от состояния компонентов
        protected override bool ActivationFunc()
        {
            // Например, активен, если хотя бы один компонент активен
            foreach (var comp in components)
            {
                if (comp != null && !comp.IsEnable)
                    return false;
            }
            return true;
        }
    }
}

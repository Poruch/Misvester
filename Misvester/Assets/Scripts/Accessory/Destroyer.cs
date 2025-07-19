using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Accessory
{
    /// <summary>
    /// Класс предназначенный для удаления объектов, использует паттерн синглтон
    /// </summary>
    public class Destroyer
    {
        int countInstance = 0;
        static Destroyer instance;
        public static Destroyer Instance
        {
            get
            {
                if (instance == null)
                    instance = new Destroyer();
                return instance;
            }
        }

        Dictionary<GameObject, Timer> destroyed = new Dictionary<GameObject, Timer>();
        private Destroyer()
        {
            countInstance++;
            if (countInstance > 1)
                Debug.LogWarning("Destroyer should not be more one");
        }

        /// <summary>
        /// Удаляет объект по истечению таймера
        /// </summary>
        /// <param name="gameObject"> Объект который удалиться когда таймер дойдет до конца</param>
        /// <param name="timer"> таймер</param>
        public void Destroy(GameObject gameObject, Timer timer)
        {
            destroyed.Add(gameObject, timer);
        }


        /// <summary>
        /// Удаляет объект
        /// </summary>
        /// <param name="gameObject"></param>
        public void Destroy(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
        }

        /// <summary>
        /// Используется в методе Update unity для подсчета времени
        /// </summary>
        public void DestroyAll()
        {
            var keys = destroyed.Keys.ToArray();
            foreach (GameObject @object in keys)
            {
                GameObject.Destroy(@object);
                destroyed.Remove(@object);
            }
        }
        public void Update()
        {
            var keys = destroyed.Keys.ToArray();
            foreach (GameObject @object in keys)
            {
                if (destroyed[@object].IsTime)
                {
                    GameObject.Destroy(@object);
                    destroyed.Remove(@object);
                }
            }
            //foreach (Timer timer in destroyed.Values)
            //{                               
            //    timer.Tick();
            //}
        }


    }
}

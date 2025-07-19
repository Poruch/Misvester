using System;

namespace Assets.Scripts.Accessory
{
    /// <summary>
    /// Структура для подсчета прошедшего времени 
    /// </summary>
    [Serializable]
    public abstract class Timer
    {
        protected Timer(float time, bool isTime = false)
        {
            Time = time;
            DeltaTime = 0;
            this.isTime = isTime;
            isStopped = false;
        }

        protected Timer(Timer timer)
        {
            Time = timer.Time;
            DeltaTime = timer.DeltaTime;
            isTime = timer.isTime;
            isStopped = timer.isStopped;
        }

        public float Time;

        [NonSerialized]
        public float DeltaTime;
        private bool isTime;
        private bool isStopped;

        public bool IsTime
        {
            get
            {
                if (isTime)
                {
                    isTime = false;
                    DeltaTime = 0;
                    return true;
                }
                else
                    return false;
            }
        }

        public bool IsStopped { get => isStopped; set => isStopped = value; }


        /// <summary>
        /// usable in Update func
        /// </summary>
        protected void Tick()
        {
            if (DeltaTime >= Time)
            {
                isTime = true;
            }
            if (!IsStopped)
                DeltaTime += UnityEngine.Time.deltaTime;
        }

        public void Reset()
        {
            DeltaTime = 0;
            isTime = false;
        }

        public float GetRatio()
        {
            return DeltaTime / Time;
        }
    }
}

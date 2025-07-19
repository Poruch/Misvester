using Assets.Scripts.Accessory;
using System.Collections.Generic;

namespace Assets.Scripts.GeneralGame
{
    /// <summary>
    /// Класс для создания и управления всеми таймерами, использует паттерн одиночка
    /// </summary>
    internal class TimeManager
    {
        List<TimerConstruct> timers = new List<TimerConstruct>();

        static TimeManager instance;
        public static TimeManager Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                instance = new TimeManager();
                return instance;
            }
        }

        private TimeManager()
        {

        }

        public Timer CreateTimer(float time, bool isTime = false)
        {
            TimerConstruct timer = new TimerConstruct(time, isTime);
            timers.Add(timer);
            return timer;
        }
        public Timer CreateTimer(Timer timer)
        {
            TimerConstruct newTimer = new TimerConstruct(timer);
            timers.Add(newTimer);
            return newTimer;
        }
        public void Update()
        {
            foreach (TimerConstruct timer in timers)
            {
                timer.Tick();
            }
        }
        public void AllReset()
        {
            foreach (TimerConstruct timer in timers)
            {
                timer.Reset();
            }
        }

        private class TimerConstruct : Timer
        {
            public TimerConstruct(float time, bool isTime) : base(time, isTime) { }
            public TimerConstruct(Timer timer) : base(timer) { }

            public new void Tick()
            {
                base.Tick();
            }
        }
    }
}

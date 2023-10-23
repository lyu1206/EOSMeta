using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Service
{
    using Objects;
    public class Timer
    {
        public Timer(float interval,int repeatcount,Action func,Func<bool> condition)
        {
            _interval = interval;
            _repeatcount = repeatcount;
            _schedule_func = func;
            _condition = condition;
            if (_repeatcount == 0)
                _runforever = true;
        }
        private float _interval;
        private float _elspase;
        private int _repeatcount;
        private bool _runforever;
        private bool _paused;
        private Action _schedule_func;
        private bool _unschedule;
        private Func<bool> _condition;
        public bool Paused { get => _paused; set => _paused = value; }
        public float Elapse { get => _elspase; set => _elspase = value; }
        public float Interval => _interval;
        public bool Runforever => _runforever;
        public Action Func => _schedule_func;
        public Func<bool> Condition => _condition;
        public int RepeatCount { get => _repeatcount; set => _repeatcount = value; }
        public bool Unschedule { get => _unschedule; set => _unschedule = value; }
    }
    public class Scheduler : ReferPlayer
    {
        private int _keyindex;
        private Dictionary<int, Timer> _willscheduletimers = new Dictionary<int, Timer>();
        private Dictionary<int, Timer> _timers = new Dictionary<int, Timer>();
        private Dictionary<int, Timer> _umscheduletimers = new Dictionary<int, Timer>();
        private int InnerKeyMaker()
        {
            _keyindex++;
            return _keyindex;
        }
        private Timer InnerMakeTimer(Action func,float interval,int repeatcount,Func<bool> condition)
        {
            var timer = new Timer(interval,repeatcount,func, condition);
            return timer;
        }
        private int InnterScheduler(Action func,float interval,int repeatcount, Func<bool> condition)
        {
            var v = InnerMakeTimer(func, interval, repeatcount, condition);
            var key = InnerKeyMaker();
            _willscheduletimers.Add(key, v);
            return key;
        }
        private void InnterRunTimer(Timer timer,float dt)
        {
            if (!timer.Paused)
            {
                timer.Elapse += dt;
                if (timer.Interval<=timer.Elapse)
                {
                    timer.Func?.Invoke();
                    timer.Elapse = 0;
                    if (!timer.Runforever)
                    {
                        timer.RepeatCount -= 1;
                    }
                }
            }
        }
        private void InnerUnSchedule(int key)
        {
            if (_timers.ContainsKey(key))
                _timers[key].Unschedule = true;
            _umscheduletimers.Remove(key);
            _willscheduletimers.Remove(key);
        }
        private void InnerCancelCheckTimner(int key,Timer timer)
        {
            if (!timer.Runforever)
            {
                if (timer.RepeatCount <= 0)
                    InnerUnSchedule(key);
            }
            if (timer.Condition?.Invoke() == true)
                InnerUnSchedule(key);
        }
        public void UnSchedule(int key)
        {
            InnerUnSchedule(key);
        }
        public int Schedule(Action func)
        {
            return InnterScheduler(func, 0.0f, 0, null);
        }
        public int ScheduleOnCondition(Action func,Func<bool> condition)
        {
            return InnterScheduler(func, 0.0f, 0, condition);
        }
        public int ScheduleOnce(Action func)
        {
            return InnterScheduler(func, 0.0f, 1,null);
        }
        public int ScheduleInterval(Action func,float interval)
        {
            return InnterScheduler(func, interval, 0,null);
        }
        public int ScheduleDetail(Action func,float interval,int repeat)
        {
            return InnterScheduler(func, interval, repeat,null);
        }
        public void Clear()
        {
            _willscheduletimers.Clear();
            _timers.Clear();
            _umscheduletimers.Clear();
        }
        public void Update(float delta)
        {
            foreach(var wtimer in _willscheduletimers)
            {
                _timers.Add(wtimer.Key, wtimer.Value);
            }
            _willscheduletimers.Clear();
            foreach (var timer in _timers)
            {
                if (!timer.Value.Unschedule)
                {
                    InnterRunTimer(timer.Value, delta);
                    InnerCancelCheckTimner(timer.Key, timer.Value);
                }
                if (timer.Value.Unschedule)
                    _umscheduletimers.Add(timer.Key, timer.Value);
            }
            foreach(var unschedule in _umscheduletimers)
            {
                _timers.Remove(unschedule.Key);
            }
            _umscheduletimers.Clear();
        }
    }
}
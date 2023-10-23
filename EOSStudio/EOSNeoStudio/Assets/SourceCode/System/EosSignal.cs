using System.Collections;
using System.Collections.Generic;
using Eos.Objects;
using NLua;
using UnityEngine;

namespace Eos.Signal
{
    public class EosSignal
    {
        public interface ISignal
        {
            void Invoke(EosObjectBase sender,object[] args);
        }
        public class SystemSignal : ISignal
        {
            public delegate void SystemSignalDelegate(EosObjectBase sender, object[] args);

            private SystemSignalDelegate _function;
            public SystemSignalDelegate Function => _function;

            public SystemSignal(SystemSignalDelegate func)
            {
                _function = func;
            }

            public void Invoke(EosObjectBase sender,object[] args)
            {
                _function?.Invoke(sender, args);
            }
        }
        public class UserSignal : ISignal
        {
            private LuaFunction _function;

            public UserSignal(LuaFunction func)
            {
                _function = func;
            }
            public void Invoke(EosObjectBase sender,object[] args)
            {
                _function?.Call(args);
            }
        }
        private List<ISignal> _signals;
        private List<ISignal> _removedsignals;
        public void Connect(LuaFunction function)
        {
            _signals = _signals ?? new List<ISignal>();
            _signals.Add(new UserSignal(function));
        }

        public void Connect(SystemSignal.SystemSignalDelegate func)
        {
            _signals = _signals ?? new List<ISignal>();
            // if (_signals.Exists(s => s is SystemSignal ss && ss.Function == func))
            //     return;
            _signals.Add(new SystemSignal(func));
        }

        public void DisConnect(SystemSignal.SystemSignalDelegate func)
        {
            var signals = new List<ISignal>(_signals);
            _removedsignals = _removedsignals ?? new List<ISignal>();
            // if (_removedsignals.Exists(s => s is SystemSignal ss && ss.Function == func))
            //     return;
            foreach (var it in signals)
            {
                if ((it is SystemSignal signal) && signal.Function == func)
                {
                    _removedsignals.Add(signal);
                }
            }
        }

        public void Invoke(EosObjectBase sender,params object[] args)
        {
            if (_signals==null)
                return;
            if (_removedsignals != null)
            {
                _removedsignals.ForEach(it => _signals.Remove(it));
                _removedsignals.Clear();
                _removedsignals = null;
            }
            _signals.ForEach(it=>it.Invoke(sender,args));
        }
    }
}
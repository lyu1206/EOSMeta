using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Eos.Script
{
    using Objects;
    public class WaitCondition : CustomYieldInstruction
    {
        private Func<bool> _condition;
        public WaitCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override bool keepWaiting => !_condition();
    }
    public interface IScript
    {
        EosObjectBase script{get;set;}
        IEnumerator Body();
        void Stop();
        void Finish();
        bool Enable { set; }
    }
    sealed class InGameScriptAttribute : Attribute
    {
        public InGameScriptAttribute()
        {
        }
    }

    public static class IngameScriptContainer
    {
        private static List<Type> _scriptables;

        public static void Initialize()
        {
            var type = typeof(IScript);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x =>
                {
                    var atrribs = x.GetCustomAttributes(false);
                    var scriptableattrib = atrribs.Find(a => a.GetType() == typeof(InGameScriptAttribute));
                    if (scriptableattrib == null)
                        return false;
                    return x.IsClass && type.IsAssignableFrom(x);
                });
            _scriptables = new List<Type>(types);
        }

        public static IScript GetScript(Type type)
        {
            var scripttype = _scriptables.Find(t => t.Name == type.Name);
            if (scripttype == null)
            {
                throw new Exception("try to get unknown type.." + type.Name);
            }

            return Activator.CreateInstance(scripttype) as IScript;
        }

        public static IScript GetScript<T>() where T : class
        {
            return GetScript(typeof(T));
        }

        public static IScript GetScript(string name,EosObjectBase scriptobj)
        {
            var scripttype = _scriptables.Find(t => t.Name == name);
            if (scripttype == null)
            {
                throw new Exception("try to get unknown type.." + name);
            }
            var script = Activator.CreateInstance(scripttype) as IScript;
            script.script = scriptobj;
            return script;
        }
    }
}
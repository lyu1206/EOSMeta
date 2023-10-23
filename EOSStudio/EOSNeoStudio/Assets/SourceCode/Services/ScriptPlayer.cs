using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Eos.Service
{
    using Objects;
    using Script;
    public class ScriptPlayer : ReferPlayer
    {
        private List<IScript> _runningscriptlist = new List<IScript>();
        private Dictionary<int, IScript> _runningscripts = new Dictionary<int, IScript>();
        private IEnumerator ScriptRunning(IScript script)
        {
            yield return Ref.Coroutine.OnCoroutineStart(script.Body());
            UnRegistScript(script);
        }
        public void RegistScript(IScript script)
        {
            if (script == null)
                return;
            if (_runningscriptlist.Contains(script))
                return;
            Ref.Coroutine.OnCoroutineStart(ScriptRunning(script));
            var runid = Ref.Coroutine.NowID;
            _runningscriptlist.Add(script);
            _runningscripts.Add(runid, script);
        }
        public void UnRegistScript(IScript script)
        {
            if (script == null)
                return;
            script.Finish();
            var removeids = new List<int>();
            foreach(var it in _runningscripts)
            {
                if (it.Value == script)
                {
                    Ref.Coroutine.OnStopCoroutine(it.Key);
                    _runningscripts.Remove(it.Key);
                    break;
                }
            }
            _runningscriptlist.Remove(script);
        }
    }
}
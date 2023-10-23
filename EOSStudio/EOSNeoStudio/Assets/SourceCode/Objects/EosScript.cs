using System;
using System.IO;
using MessagePack;
using UnityEngine;

namespace Eos.Objects
{
    using Ore;
    using Script;
    [System.Serializable]
    [EosObject]
    [MessagePackObject]
    public partial class EosScript : EosObjectBase
    {
        [Key(1002)]public string scriptname;
        [RequireMold("ScriptMold")]
        [Inspector("Ore","Script")]
        [Key(1000)] public OreReference ScriptOre { get; set; }
        [Key(1001)] public string LuaScript;
        private int _scriptID = -1;
        private IScript _script;
        private int _scriptcoroutineid;

        protected override void OnActivate(bool active)
        {
            if (!CanDo())
                return;
            Do();
        }

        protected virtual bool CanDo()
        {
            return Ref.SessionManager.IsServer;
        }
        protected void Do()
        {
            if (!Ref.IsPlaying)
                return;
            if (!string.IsNullOrEmpty(scriptname))
            {
                var path = $"{Application.streamingAssetsPath}/{scriptname}";
                LuaScript = File.ReadAllText(path);
            }
            if (!string.IsNullOrEmpty(LuaScript))
            {
                DoString(_active);
                return;
            }
            var ore = ScriptOre.GetOreObject();
            if (ore == null)
                return;
            scriptname = ore.name;
            _script = _script??IngameScriptContainer.GetScript(scriptname, this);
            if (_active)
            {
                Ref.ScriptPlayer.RegistScript(_script);
            }
            else
            {
                {
                    _script.Enable = false;
                    Ref.ScriptPlayer.UnRegistScript(_script);
                }
            }
//            _scriptcoroutineid = Ref.Coroutine.OnCoroutineStart(script.Body());
        }
        private void DoString(bool active)
        {
            if (active)
            {
                if (!string.IsNullOrEmpty(LuaScript))
                {
                    if (_scriptID == -1)
                        DoLuaScript();
                    else
                        PauseResumeScript(false);
                    return;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(LuaScript))
                {
                    PauseResumeScript(true);
                }

            }
        }
        private void DoLuaScript()
        {
            try
            {
                _scriptID = Ref.LuaPlayer.RegistRoutine(this, LuaScript);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private void PauseResumeScript(bool pause)
        {
            if (pause)
                Ref.LuaPlayer.PauseRoutine(_scriptID);
            else
                Ref.LuaPlayer.ResumeRoutine(_scriptID);
        }
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosScript targetscript))
                return;
            targetscript.LuaScript = LuaScript;
            targetscript.ScriptOre = ScriptOre;
            targetscript.scriptname = scriptname;
            base.OnCopyTo(target);
        }
        public override void OnDestroy()
        {
            if (_scriptID != -1)
                Ref.LuaPlayer.UnRegistRoutine(_scriptID);
            _script?.Stop();
            Ref.ScriptPlayer.UnRegistScript(_script);
        }
    }
}
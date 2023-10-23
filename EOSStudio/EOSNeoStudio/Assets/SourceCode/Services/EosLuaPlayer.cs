using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Eos.Objects;
using UnityEngine;
using NLua;

namespace EosLuaPlayer
{
    public static class PrimaryLuaFunction
    {
        public static void BindFunction(Lua state)
        {
            var type = typeof(PrimaryLuaFunction);
            state.RegisterFunction("print", type.GetMethod("Print", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            state.RegisterFunction("InstanceNew", type.GetMethod("CreateEosInstance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
        }
        public static void Print(params object[] args)
        {
            var b = new StringBuilder();
            args.ForEach(it =>
            {

                if (it != null)
                    b.Append(it.ToString());
                else
                    b.Append("null");
            });
            UnityEngine.Debug.Log(b.ToString());
        }
        public static EosObjectBase CreateEosInstance(string typename)
        {
            var type = Type.GetType($"Eos.Objects.{typename}");
            var instance =Eos.ObjectFactory.CreateInstance(type);
            var om = instance.Ref.ObjectManager;
            var synctype = instance.Ref.SessionManager.IsServer ? ObjectType.Sync : ObjectType.NonSync; 
            om.SetRegistType(synctype);
            om.RegistObject(instance);
            instance.OnCreate();
            return instance;
        }
    }
    public class EosLuaPlayer : Eos.Objects.ReferPlayer
    {
        private const string str_loadmodule = "loadmodule";
        public Lua LuaMain { get; private set; } = null;
        private LuaFunction _updater = null;
        private LuaFunction _registroutine = null;
        private LuaFunction _unregistroutine = null;
        private LuaFunction _pauseroutine = null;
        private LuaFunction _resumeroutine = null;
        private LuaFunction _loadmodule = null;

        public void Init()
        {
            LuaMain = new Lua();
            PrimaryLuaFunction.BindFunction(LuaMain);
            LoadBuiltInLuas();


            _updater = LuaMain["player.Update"] as LuaFunction;
            _registroutine = LuaMain["player.RegistRoutine"] as LuaFunction;
            _unregistroutine = LuaMain["player.UnRegistRoutine"] as LuaFunction;
            _pauseroutine = LuaMain["player.PauseRoutine"] as LuaFunction;
            _resumeroutine = LuaMain["player.ResumeRoutine"] as LuaFunction;

            LuaMain["Services"] = Ref.Solution;

            var testpawn = new Eos.Objects.EosScript{ Name = "Pawn" };
            testpawn.LuaScript =
                @"
                    local this = _this_object
                    print(tostring(this))
                    local i = 0
                    local time = 0
                    while i<40 do
                        i = i + 1
                        print('count:',i,' ',time,' obj - ',tostring(this),' objname:',this.Name)
                        time = time + Time.deltaTime
                        coroutine.yield()
                    end
                ";
//            testpawn.Activate(true);


            var testscript = new Eos.Objects.EosScript { Name = "Script" };
            testscript.LuaScript =
                @"
                    local i = 0
                    local this = _this_object
                    while i<60 do
                        i = i + 1
                        print('second count:',i,' obj - ',tostring(this),' objname:',this.Name)
                        coroutine.yield()
                    end
                ";
//            testscript.Activate(true);
        }
        public int RegistRoutine(Eos.Objects.EosObjectBase owner,string code)
        {
            string chunk = $"{owner.Name}-{owner.ObjectID}";
            var ret = _registroutine.Call(owner, code, chunk);
            return 0;
        }
        public void UnRegistRoutine(int index)
        {
            _unregistroutine?.Call(index);
        }
        public void PauseRoutine(int index)
        {
            _pauseroutine.Call(index);
        }
        public void ResumeRoutine(int index)
        {
            _resumeroutine.Call(index);
        }
        public void Update()
        {
            LuaMain["Time.deltaTime"] = Time.deltaTime;
            _updater?.Call();
        }
        public void LateUpdate()
        {

        }
        private void LoadModule(string code,string name,string chunkkey)
        {
            _loadmodule = _loadmodule ?? LuaMain[str_loadmodule] as LuaFunction;
            _loadmodule.Call(code, name, chunkkey);
        }
        private void LoadBuiltInLuas()
        {
            var luapath = $"{Application.streamingAssetsPath}/BuiltInLua";
            var files = Directory.GetFiles(luapath, "*.lua");
            files = files.Sort((a, b) => 
            {
                var sa = a.Split('_').Length;
                var sb = b.Split('_').Length;
                if (sa > sb) return 1;
                else if (sa < sb) return -1;
                return 0;
            });
            foreach(var file in files)
            {
                var chunkname = Path.GetFileName(file);
                var name = Path.GetFileNameWithoutExtension(file);
                var attribute = name.Split('_');
                var code = File.ReadAllText(file, Encoding.UTF8);
                var ismodule = attribute.Length > 1;
                if (!ismodule)
                    LuaMain.DoString(code);
                else
                    LoadModule(code, attribute[0], chunkname);
            }
        }
    }
}
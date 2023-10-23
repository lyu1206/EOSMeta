using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eos.Network;
using Eos.Objects;
using Unity.VisualScripting;

//using Ludiq;

public class CommandsAttribute : Attribute
{
    public int cmd;
    public CommandsAttribute(int cmd_)
    {
        cmd = cmd_;
    }
}

public static class CommandsCache
{
    private static Dictionary<int,Tuple<object,MethodInfo>> _commandscache = new Dictionary<int, Tuple<object, MethodInfo>>();
    public static void Init(EosPlayer.EosPlayer player)
    {
        GetCommand(player.ObjectManager);
        GetCommand(player.Players);
    }

    public static void Invoke(CmdPacket packet)
    {
        if (!_commandscache.ContainsKey(packet.cmd))
            throw new Exception($"command:{(S2C.Commands)packet.cmd} not define method");
        var objmng = EosPlayer.EosPlayer.Instance.ObjectManager;
        var command = _commandscache[packet.cmd]; 
        command.Item2.Invoke(command.Item1,new object[]{packet});
    }

    private static void GetCommand(object obj)
    {
        var type = obj.GetType();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly);
        foreach (var methodInfo in methods.Where(m => m.HasAttribute<CommandsAttribute>()).Select(minfo => minfo))
        {
            var attribute = methodInfo.GetCustomAttribute<CommandsAttribute>();
            if (_commandscache.ContainsKey(attribute.cmd))
            {
                if (attribute.cmd<10000) throw new Exception($"duplicated command:{(S2C.Commands) attribute.cmd}");
                else throw new Exception($"duplicated command:{(C2S.Commands) attribute.cmd}");
            }
            _commandscache.Add(attribute.cmd, new Tuple<object, MethodInfo>(obj,methodInfo));
        }
    }
}
namespace S2C
{
    public enum Commands
    {
        CreateObject = 1000,
        SyncObject = 1001,
        CreatePlayers = 1002,
        NotifyObjectProperty = 1003,
        NotifyObjectAction = 1004,
    }
}

namespace C2S
{
    public enum Commands
    {
        RequestPlayer = 10000,
        NotifyObjectAction = 1004,
    }
}
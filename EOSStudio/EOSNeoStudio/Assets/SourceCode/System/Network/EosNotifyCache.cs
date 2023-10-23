using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eos;
using UnityEngine;

public static class EosNotifyCache 
{
    private static ConcurrentDictionary<int,PropertyInfo> _tagpropertycaches = new ConcurrentDictionary<int, PropertyInfo>(); 
    private static ConcurrentDictionary<int,MethodInfo> _notifymethodscaches = new ConcurrentDictionary<int, MethodInfo>(); 
    private static ConcurrentDictionary<int,MethodInfo> _notifyobejctactioncaches = new ConcurrentDictionary<int, MethodInfo>(); 
    public static PropertyInfo GetNotifyProperty(EosNotifyObject obj, int notify)
    {
        PropertyInfo notifypropertyinfo = null;
        if (_tagpropertycaches.TryGetValue((int) notify,out notifypropertyinfo))
            return notifypropertyinfo;
        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var prop in properties)
        {
            var tttt = prop.GetCustomAttributes<EosNotifyTagAttribute>(true);
            var t2 = tttt.SingleOrDefault();
        }

        var test = properties.Select(p =>
                new Tuple<EosNotifyTagAttribute, PropertyInfo>(
                    p.GetCustomAttributes<EosNotifyTagAttribute>(true).SingleOrDefault(), p))
            .Where(pp =>
            {
                if (pp.Item1 == null || pp.Item1.Tag != notify)
                    return false;
                _tagpropertycaches.TryAdd((int) notify, pp.Item2);
                notifypropertyinfo = pp.Item2;
                return true;
            }).SingleOrDefault();
        var test2  = properties.Select(p => new Tuple<EosKeyTag,PropertyInfo>(p.GetCustomAttributes<EosKeyTag>(true).SingleOrDefault(),p))
            .Where(pp =>
            {
                if (pp.Item1 == null || pp.Item1.Tag != notify)
                    return false;
                _tagpropertycaches.TryAdd((int) notify, pp.Item2);
                notifypropertyinfo = pp.Item2;
                return true;
            }).SingleOrDefault();
        if (notifypropertyinfo == null)
        {
            // throw new Exception($"no notify property:{(EosPropertyNotify) notify}");
            Debug.LogError($"no notify property:{(EosPropertyNotify) notify}");
            return null;
        }

        var methods = type.GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public |
                                      BindingFlags.NonPublic);
        var mmmmm = methods.Select(m =>
                new Tuple<NotifyPropertyMethod, MethodInfo>(
                    m.GetCustomAttributes<NotifyPropertyMethod>(true).SingleOrDefault(), m))
            .Where(mm =>
            {
                if (mm.Item1 == null)
                    return false;
                if (mm.Item1.Notify == (int) notify)
                {
                    _notifymethodscaches.TryAdd((int) notify, mm.Item2);
                }
                return true;
            }).ToList();
        return notifypropertyinfo;
    }
    public static MethodInfo GetNotifyObjectAction(EosNotifyObject obj, int action)
    {
        MethodInfo notifyobjectactioninfo = null;
        if (_notifyobejctactioncaches.TryGetValue((int) action,out notifyobjectactioninfo))
            return notifyobjectactioninfo;
        var type = obj.GetType();
        var methods = type.GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var mmmmm = methods.Select(m =>
                new Tuple<NotifyObjectActionMethod, MethodInfo>(
                    m.GetCustomAttributes<NotifyObjectActionMethod>(true).SingleOrDefault(), m))
            .Where(mm =>
            {
                if (mm.Item1 == null)
                    return false;
                if (mm.Item1.Action == (int) action)
                {
                    notifyobjectactioninfo = mm.Item2;
                    _notifyobejctactioncaches.TryAdd((int) action, mm.Item2);
                }
                return true;
            }).ToList();
        return notifyobjectactioninfo;
    }

    public static MethodInfo GetNotifyActionMethod(int notify)
    {
        MethodInfo notifyactionmethod = null;
        _notifymethodscaches.TryGetValue( notify, out notifyactionmethod);
        return notifyactionmethod;
    }
}

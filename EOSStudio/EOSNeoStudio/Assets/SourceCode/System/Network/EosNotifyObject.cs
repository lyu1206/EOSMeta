using System;
using System.Collections;
using System.Collections.Generic;
using Eos;
using MessagePack;
using TMPro;
using UnityEngine;

public interface INotifyObject
{
    void OnNotifyProperyChanged(EosNotifyObject sender,EosPropertyNotify notify,bool incomming);
    void OnNotifyObjectAction(EosNotifyObject sender,EosObjectAction action,bool incomming);
}
public class NotifyPropertyMethod : Attribute
{
    public int Notify;
    public NotifyPropertyMethod(int notify)
    {
        Notify = notify;
    }
}
public class NotifyObjectActionMethod : Attribute
{
    public int Action;
    public NotifyObjectActionMethod(int action)
    {
        Action = action;
    }
}

public static class ObjectNotifyInvoker
{
    public static void InvokePropertyChange(EosNotifyObject target, EosPropertyNotify notify,bool incomming)
    {
        if (target.ObjectToNotify==null)
            return;
        var notifies = new List<INotifyObject>(target.ObjectToNotify);
        notifies.ForEach(o => o.OnNotifyProperyChanged(target,notify,incomming));
    }
    public static void InvokeObjectAction(EosNotifyObject target, EosObjectAction action,bool incomming)
    {
        if (target.ObjectToNotify==null)
            return;
        var notify = new List<INotifyObject>(target.ObjectToNotify);
        notify.ForEach(o => o.OnNotifyObjectAction(target,action,incomming));
    }
}

namespace Eos
{
    using Objects;
    public class EosNotifyObject :  ReferPlayer
    {
        private HashSet<short> _notifications = new HashSet<short>();
        private HashSet<short> _changednotifications = new HashSet<short>();
        private HashSet<short> _notifymethods = new HashSet<short>();
        private List<INotifyObject> _objectToNotify = null;
        private bool _notifyOut = false;

        [IgnoreMember]
        public bool NotifyOut
        {
            get
            {
                var parent = ((EosObjectBase) this).Parent;
                if (_notifyOut)
                    return true;
                if (parent != null)
                    return parent.NotifyOut;
                return false;
            }
            set { _notifyOut = value; }
        }
        [IgnoreMember]public List<INotifyObject> ObjectToNotify => _objectToNotify;

        public delegate void NotifyObject(EosNotifyObject obj);

        public event NotifyObject Notify;
        public void NotifyPropertyChange(EosPropertyNotify notify)
        {
            // Debug.Log($"notify property changed on {((EosObjectBase)this).Name}({((EosObjectBase)this).ObjectID}) - {notify}");
            
            if (!NotifyOut)
                return;
            
            _notifications.Add((short) notify);
            _changednotifications.Add((short) notify);
            ObjectNotifyInvoker.InvokePropertyChange(this,(EosPropertyNotify)notify,false);
            Notify?.Invoke(this);
        }
        public void SendObjectAction(EosObjectAction action,params object[]parameters)
        {
            ObjectNotifyInvoker.InvokeObjectAction(this,action,false);
            
            if (!NotifyOut)
                return;
            
            var packet = new NotifyObjectActionPacket();
            packet.objectid = ((EosObjectBase)this).ObjectID;
            packet.notifyaction = (short)action;
            packet.parameters = parameters;
            var data = MessagePackSerializer.Serialize(packet);
            Ref.SessionManager.SendPacket((int) C2S.Commands.NotifyObjectAction ,data );
        }

        public void OnObjectAction(EosObjectAction action)
        {
            Ref.ObjectManager.ObjectAction(this,action);
        }
        public void AddObjectToNotify(INotifyObject obj)
        {
            _objectToNotify = _objectToNotify ?? new List<INotifyObject>();
            if (_objectToNotify.Contains(obj))
                return;
            _objectToNotify.Add(obj);
        }

        public void DeleteObjectToNotify(INotifyObject obj)
        {
            _objectToNotify = _objectToNotify ?? new List<INotifyObject>();
            _objectToNotify.Remove(obj);
        }


        public NotifyPropertyPacket MakeNotifications()
        {
            if (_notifications.Count == 0)
                return null;
            NotifyPropertyPacket packet = new NotifyPropertyPacket();
            packet.properties = new List<NotifyPropertyPacket.NotifyPropertyItem>();
            packet.objectid = (this as EosObjectBase).ObjectID;
            foreach (var n in _notifications)
            {
                var p = EosNotifyCache.GetNotifyProperty(this,n);
                if (p==null)
                    continue;
                var val = p.GetValue(this);
                packet.properties.Add(new NotifyPropertyPacket.NotifyPropertyItem {notify = n, value = val});
            }
            _notifications.Clear();
            return packet;
        }
    }
}

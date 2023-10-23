using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Eos.Objects.Description;

namespace Eos.Objects
{
    using UI;
    using Service;
    using Service.AI;
    using MessagePack;

    [System.Serializable]
    [EosObject]
    public partial class EosModel : EosTransformActor
    {
        [IgnoreMember] public EosTransformActor PrimaryActor;
        public EosModel()
        {
        }
        protected override void OnActivate(bool active)
        {
            PrimaryActor = FindChild<EosTransformActor>();
        }
#if UNITY_EDITOR
        [MessagePack.IgnoreMember]
        public override string Name 
        { 
            get => base.Name; 
            set
            {
                base.Name = value;
                if (_transform == null)
                    return;
                _transform.Name = value;
            }
        }
        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            if (!(_parent is ITransform transactor))
                return;
            _transform.SetParent(transactor.Transform);
        }

#endif
    }
    public class ReferPlayer
    {
        [IgnoreMember]public EosPlayer.EosPlayer Ref => EosPlayer.EosPlayer.Instance;
    }
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(EosCamera))]
    [Union(1, typeof(EosTransformActor))]
    [Union(2, typeof(EosHumanoid))]
    [Union(3, typeof(GUIService))]
    [Union(4, typeof(AIService))]
    [Union(5, typeof(Solution))]
    [Union(6, typeof(Workspace))]
    //[Union(7, typeof(EosFsm))]
    [Union(8, typeof(EosLight))]
    [Union(9, typeof(EosMeshObject))]
    [Union(10, typeof(EosPawnActor))]
    [Union(11, typeof(EosScript))]
    [Union(12, typeof(EosTool))]
    [Union(13, typeof(EosModel))]
    [Union(14, typeof(TerrainService))]
    [Union(15, typeof(EosTerrain))]
    [Union(16, typeof(StarterPlayer))]
    [Union(17, typeof(StarterPack))]
    [Union(18, typeof(EosUIObject))]
    [Union(19, typeof(EosTextMesh))]
    [Union(20, typeof(EosCollider))]
    [Union(21, typeof(EosBone))]
    [Union(22, typeof(EosGear))]
    [Union(23, typeof(EosShape))]
    [Union(24, typeof(Player))]
    [Union(25, typeof(EosServerScript))]
    public abstract partial class EosObjectBase : EosNotifyObject , IMessagePackSerializationCallbackReceiver
    {
        public EosObjectBase()
        {
        }
        [IgnoreMember]public EventHandler OnReadyEvent;
        [IgnoreMember]public uint ObjectID;
        private string _name;
        protected bool _active = true;
        protected bool _ready = true;
        [IgnoreMember] public bool Ready => _ready;
        [Key(1)] public List<EosObjectBase> _children  = new List<EosObjectBase>();
        [IgnoreMember] public int ChildCount => _children.Count;
        [IgnoreMember] public List<EosObjectBase> Children => _children;
        //[Inspector("Basic","Name")]
        [Key(2)]public virtual string Name{get=>_name;set=>_name = value;}
        [Inspector("Basic", "Active")]
        [Key(3)]public bool Active 
        { 
            get => _active;
            set
            {
                if (_active == value)
                    return;
                _active = value;
                Activate(value);
            }
        }
        [IgnoreMember]
        public bool ActiveInHierachy
        {
            get
            {
                if (!Active) return false;
                if (Parent == null) return Active;
                return Active && Parent.ActiveInHierachy;
            }
        }
        [IgnoreMember] protected EosObjectBase _parent;

        [IgnoreMember]
        public EosObjectBase Parent
        {
            get=>_parent;
            set
            {
                value.AddChild(this);
            }
        }

        public async void AddChild(EosObjectBase obj)
        {
            // if (obj.ObjectID==0)
            //     Ref.ObjectManager.RegistObject(obj);
            if (obj==null)
                return;
            if (obj._parent !=null)
                obj._parent.RemoveChild(obj);
            obj._parent = this;
            _children.Add(obj);
            if (!Ref.IsPlaying)
                return;
            if (!obj._ready)
            {
                await TaskExtension.WaitUntil(obj, (o) => o._ready);
//                Debug.Log($"_____________________________Child added:{Name} -> {obj.Name}");
            }
            obj.AncestryChanged();
            OnChildAdded(obj);
        }
        public virtual void OnCopyTo(EosObjectBase target)
        {
            // if (target.ObjectID == 0)
            //     Ref.ObjectManager.RegistObject(target, ObjectType.RunTime);
            target.Name = Name;
        }
        public virtual void OnCreate()
        {
            _children = _children ?? new List<EosObjectBase>();
        }
        public virtual void Activate(bool active,bool recursivechild = true)
        {
            OnActivate(ActiveInHierachy);
            if (recursivechild)
            {
                for (int i=0;i<_children.Count;i++)
                    _children[i].Activate(active, recursivechild);
                // foreach (var child in _children)
                //     child.Activate(active, recursivechild);
            }
        }
        public void Destroy(bool silence = true)
        {
            if (silence && _parent!=null)
                _parent.RemoveChild(this);
            Ref.ObjectManager.UnRegistObject(this);
            OnDestroy();
            var children = new List<EosObjectBase>(_children);
            foreach (var child in children)
                child.Destroy(false);
        }
        public virtual void OnDestroy()
        {
        }
        public virtual void StartPlay()
        {
            OnStartPlay();
            foreach(var child in _children)
                child.StartPlay();
        }
        protected virtual void OnActivate(bool active){}
        protected virtual void OnStartPlay(){}
        public virtual void Update(float delta){}
        public void RemoveChild(EosObjectBase obj)
        {
            _children.Remove(obj);
            OnChildRemoved(obj);
        }
        public virtual void OnChildAdded(EosObjectBase child)
        {

        }
        public virtual void OnChildRemoved(EosObjectBase child)
        {

        }
        public void AncestryChanged()
        {
            OnAncestryChanged();
            foreach(var child in _children)
                child.AncestryChanged();
        }
        public void IterChilds<T>(Action<T> action, bool isrecursive = false) where T : EosObjectBase
        {
            _children.ForEach(it =>
            {
                if (it is T obj)
                    action(obj);
                if (isrecursive && it != null)
                    it.IterChilds(action, isrecursive);
            });
        }
        public void IterChilds(Action<EosObjectBase> action,bool isrecursive = false)
        {
            _children.ForEach(it =>
            {
                action(it);
                if (isrecursive && it!=null)
                    it.IterChilds(action,isrecursive);
            });
        }
        public EosObjectBase FindChild(string name)
        {
            foreach (var child in _children)
            {
                if (child.Name == name)
                    return child;
            }
            return null;
        }
        public T FindDeepChild<T>() where T : EosObjectBase
        {
            var find = FindChild<T>();
            if (find != null)
                return find;
            foreach (var child in _children)
            {
                find = child.FindDeepChild<T>();
                if (find != null)
                    return find;
            }
            return null;
        }
        public T FindDeepChild<T>(string name) where T : EosObjectBase
        {
            var find = FindChild<T>(name);
            if (find != null)
                return find;
            foreach (var child in _children)
            {
                find = child.FindDeepChild<T>(name);
                if (find != null)
                    return find;
            }
            return null;
        }
        public T FindChild<T>() where T : EosObjectBase
        {
            foreach(var child in _children)
            {
                if (child is T)
                    return (T)child;
            }
            return null;
        }
        public List<T> FindChilds<T>() where T : EosObjectBase
        {
            var retlist = new List<T>();
            foreach (var child in _children)
            {
                if (child is T)
                    retlist.Add(child as T);
            }
            return retlist;
        }
        public T FindChild<T>(string name) where T :EosObjectBase
        {
            foreach(var child in _children)
            {
                if (child is T && child.Name == name)
                    return (T)child;
            }
            return null;
        }
        public T FindInParent<T>() where T :EosObjectBase
        {
            var parent = _parent;
            while(parent!=null)
            {
                if (parent is T)
                    return parent as T;
                parent = parent._parent;
            }
            return null;
        }
        public virtual void OnAncestryChanged()
        {
        }
        public bool IsDescendantOf(Type type)
        {
            var parent = this;
            while(parent!=null)
            {
                if (parent.GetType() == type)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }
        public EosObjectBase Clone(EosObjectBase parent)
        {
            var type = ObjectFactory.GetRegistType(this);
            //if (type == ObjectType.RunTime)
            //    return null;
            var synctype = Ref.SessionManager.IsServer ? ObjectType.Sync : ObjectType.NonSync; 
            Ref.ObjectManager.SetRegistType(synctype);
            
            var copy = ObjectFactory.CreateInstance(this.GetType());
            copy.OnCreate();
            OnCopyTo(copy);
            Ref.ObjectManager.RegistObject(copy);
            parent?.AddChild(copy);
            foreach (var child in _children)
            {
                var childclone = child.Clone(copy);
                if (childclone == null)
                    continue;
            }
            return copy;
        }

        public virtual void OnBeforeSerialize()
        {
//            throw new NotImplementedException();
        }

        public virtual void OnAfterDeserialize()
        {
            OnCreate();
            Ref.ObjectManager.RegistObject(this);
            if (_children == null)
                return;
            var childrens = new List<EosObjectBase>(_children);
            _children.Clear();
            foreach (var it in childrens)
            {
                if (it == null)
                    continue;
                AddChild(it);
            }
            //            throw new NotImplementedException();
        }
    }
}

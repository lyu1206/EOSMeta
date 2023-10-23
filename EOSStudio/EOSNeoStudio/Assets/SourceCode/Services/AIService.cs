using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Service.AI
{
    using Objects;

    [NoCreated]
    public class AIObject : EosObjectBase
    {

    }
    public class Chain
    {
        Chain _next;
        public virtual float GetResult(EosHumanoid a,EosHumanoid b)
        {
            var val = OnGetResult(a,b);
            if (val == 0)
                return 0;
            if (_next != null)
            {
                var result = _next.GetResult(a, b);
                if (result == 0)
                    return 0;
                val += result;
            }
            return val;
        }
        public virtual float OnGetResult(EosHumanoid a, EosHumanoid b)
        {
            return 1;
        }
        public Chain AddNext(Chain next)
        {
            _next = next;
            return next;
        }
    }
    public class LessDistance :Chain
    {
        public float Max = 1000;
        public override float OnGetResult(EosHumanoid a, EosHumanoid b)
        {
            var pa = a.Humanoidroot;
            var pb = b.Humanoidroot;
            var distance = (pa.LocalPosition - pb.LocalPosition).magnitude;
            if (distance > Max)
                return 0;
            return distance;
        }
    }
    public class LessLeveltDelta : Chain
    {
        public int Max = 2;
        public override float OnGetResult(EosHumanoid a, EosHumanoid b)
        {
            var leveldelta = a.Level - b.Level;
            if (leveldelta < 0 || leveldelta > Max)
                return 0;
            return leveldelta + 1;
        }
    }
    [NoCreated]
    public class AIEngageObject : AIObject , ITransform
    {
        private EosHumanoid _owner;
        private List<AIEngageObject> _targets = new List<AIEngageObject>();
        private EosTransform _transform;
        private bool _haveaccespoint;
        private Vector3 _accespoint;
        public EosHumanoid Owner => _owner;

        public EosTransform Transform => _transform;

        public AIEngageObject(EosHumanoid owner)
        {
            _owner = owner;

            //var engagetarget = new GameObject("ExpectPosition");
            //_transform = engagetarget.transform;
            //_transform.gameObject.layer = LayerMask.NameToLayer("AI");
            //var modelroot = _owner.Parent as ITransform;
            //_transform.SetParent(modelroot.Transform, false);

            //var collider = owner.Humanoidroot.FindChild<EosCollider>();
            //collider.Collider.Attach(this);
            //_transform.GetComponent<Collider>().isTrigger = true;
            //_owner.Parent.AddChild(this);
        }
        public void ResetTrace()
        {
            _haveaccespoint = false;
        }
        public void AddEngage(AIEngageObject obj)
        {
            if (!_targets.Contains(obj))
                _targets.Add(obj);
        }
        public bool GetAccesPoint(AIEngageObject who,out Vector3 point)
        {
            if (who._haveaccespoint)
            {
                point = who._accespoint;
                return true;
            }
            point = Vector3.zero;
            if (_owner.AccesPlate != null)
            {
                who._owner.NavAgent.stoppingDistance = _owner.Radius*0.1f;
                who._haveaccespoint = _owner.AccesPlate.GetAccesPoint(who, out point);
                who._accespoint = point;
            }
            else
            {
                var direcion = who._owner.Humanoidroot.LocalPosition - _owner.Humanoidroot.LocalPosition;
                direcion.Normalize();
                point = _owner.Humanoidroot.LocalPosition;// + direcion * (who.Radius+target.Radius);
                who._owner.NavAgent.stoppingDistance = (who._owner.Radius + _owner.Radius) * 1.2f;
            }

            return true;
        }
        public EosHumanoid AttackableTarget()
        {
            foreach(var target in _targets)
            {
                var distance = (_owner.Humanoidroot.LocalPosition - target._owner.Humanoidroot.LocalPosition).magnitude;
                var maleedistance = _owner.Radius + target._owner.Radius;
                if (distance <= maleedistance)
                    return target._owner;
            }
            return null;
        }
        public void RemoveEngage(AIEngageObject obj)
        {
            _targets.Remove(obj);
        }
        public override void OnDestroy()
        {
            foreach (var target in _targets)
                target.RemoveEngage(this);
            Ref.Solution.AIService.UnRegistEngageObject(this);
        }
        public void Destroy()
        {
        }
        public void Engage()
        {
//             var fsm = _owner.FindChild<EosFsm>();
//             EosHumanoid finaltarget = null;
//
//             var chain = new LessDistance { Max = 1000 };
//             chain.AddNext(new LessLeveltDelta { Max = 100 });
//             var resultscore = float.MaxValue;
//             foreach(var it in _targets)
//             {
//                 var target = it._owner as EosHumanoid;
//                 var result = chain.GetResult(_owner as EosHumanoid,target);
//                 if (result == 0)
//                     continue;
//                 if (resultscore > result)
//                 {
//                     resultscore = result;
//                     finaltarget = target;
//                 }
//             }
//
//             if (finaltarget == null)
//                 return;
// //            var accesspoint = finaltarget.Ref.Solution.AIService.GetAccesPoint(_owner as EosHumanoid, finaltarget);
//             fsm.SetFsmValue("trace", true);
// //            fsm.SetFsmValue("tracetarget", finaltarget.Humanoidroot.Transform);
//             fsm.SetFsmValue("tracetarget", finaltarget);
//             fsm.FsmTransition("Trace");
//             _owner.TraceTarget = finaltarget;
//             if (_owner.AttackRange!=null)
//                 _owner.AttackRange.Enable = true;
        }
    }
    [NoCreated]
    [System.Serializable]
    [MessagePackObject]
    public class AIService : EosService
    {
        private Dictionary<uint, AIEngageObject> _enageobjects = new Dictionary<uint, AIEngageObject>();
        //private Dictionary<int, AIEngageObject> _enageobjectsByInstanceID = new Dictionary<int, AIEngageObject>();

        private AIEngageObject RegistEngageObject(EosHumanoid obj)
        {
            if (!_enageobjects.ContainsKey(obj.ObjectID))
            {
                var engageobject = new AIEngageObject(obj);
                _enageobjects.Add(obj.ObjectID, engageobject);
                //_enageobjectsByInstanceID.Add(engageobject.Transform.GetInstanceID(),engageobject);
            }
            return _enageobjects[obj.ObjectID];
        }
        public void UnRegistEngageObject(AIEngageObject obj)
        {
            _enageobjects.Remove(obj.ObjectID);
            //_enageobjectsByInstanceID.Remove(obj.Transform.GetInstanceID());
        }
        public AIEngageObject GetAIEngageObject(uint id)
        {
            if (_enageobjects.ContainsKey(id))
                return _enageobjects[id];
            return null;
        }
        //public AIEngageObject GetAIEngageObjectByInstanceID(int id)
        //{
        //    if (_enageobjectsByInstanceID.ContainsKey(id))
        //        return _enageobjectsByInstanceID[id];
        //    return null;
        //}
        public EosHumanoid GetAttackbleTarget(EosHumanoid humanid)
        {
            var engage = GetAIEngageObject(humanid.ObjectID);
            if (engage == null)
                return null;
            return engage.AttackableTarget();
        }
        public void SetAccessPlate(EosObjectBase humanoid)
        {
            var plate = new AccessPlate();
            humanoid.AddChild(plate);
        }
        public bool RegistEngage(EosHumanoid a, EosHumanoid b)
        {
            if (a.Humanoidroot.Layer == b.Humanoidroot.Layer)
                return false;
            var aiA = RegistEngageObject(a);
            var aiB = RegistEngageObject(b);
            aiA.AddEngage(aiB);
            aiB.AddEngage(aiA);
            return true;
        }
        public Vector3 GetAccesPoint(EosHumanoid who,EosHumanoid target)
        {
            var enwho = GetAIEngageObject(who.ObjectID);
            var entarget = GetAIEngageObject(target.ObjectID);
            Vector3 accesspoint;
            entarget.GetAccesPoint(enwho, out accesspoint);
            return accesspoint;
        }
    }
}
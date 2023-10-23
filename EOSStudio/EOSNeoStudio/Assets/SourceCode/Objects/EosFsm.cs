using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
//    using Bolt;
    using Ore;
    [EosObject]
    public class EosFsm : EosObjectBase
    {
        [IgnoreMember]public OreBase FSM;
        [RequireMold("FsmMold")]
        [Inspector("Ore","Fsm")]
        // [Key(331)]public OreReference FSMOre { get; set; }
        // private VariableDeclarations _values;
        private GameObject _boltobject;
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosFsm targetfsm))
                return;
            targetfsm.FSM = FSM;
            // targetfsm.FSMOre = FSMOre;
            base.OnCopyTo(target);
        }
        protected override void OnActivate(bool active)
        {
//             var fsmore = FSMOre.GetOre();
//             if (fsmore != null)
//                 FSM = fsmore.GetComponent<OreBase>();
//             var fsm = FSM.Instantiate();
//             var boltlink = fsm.GetComponent<BoltLinkOre>();
//             _values = ObjectVariables.Declarations(boltlink.gameObject, false, false);
//             _boltobject = boltlink.gameObject;
//             boltlink.SetObject(_parent);
// #if UNITY_EDITOR
//             if (_parent is ITransform trans)
//                 fsm.transform.SetParent(trans.Transform.Transform);
// #endif
        }
        public void SetFsmValue(string name,object value)
        {
            // _values[name] = value;
        }
        public void FsmTransition(string name,params object[]args)
        {
            // CustomEvent.Trigger(_boltobject, name, args);
        }
    }
}
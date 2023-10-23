using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    using Signal;
    [EosObject]
    [DescendantOf(typeof(Eos.Service.Workspace))]
    public class EosShape : EosTransformActor , IColliderEvent
    {
        private PrimitiveType _type = PrimitiveType.Capsule;
        private EosSignal _oncollisionenter;
        private EosSignal _oncollisionexit;
        private EosSignal _ontriggerenter;
        private EosSignal _ontirggerexit;
        [IgnoreMember] public EosSignal OnCollisionEnter { get =>_oncollisionenter = _oncollisionenter??new EosSignal();}
        [IgnoreMember] public EosSignal OnCollisionExit{ get=> _oncollisionexit = _oncollisionexit??new EosSignal();}
        [IgnoreMember] public EosSignal OnTriggerEnter{ get=> _ontriggerenter = _ontriggerenter??new EosSignal();}
        [IgnoreMember] public EosSignal OnTriggerExit{ get=> _ontirggerexit = _ontirggerexit??new EosSignal();}
        [Key(201)]
        public PrimitiveType Type
        {
            set
            {
                if (_type==value && _transform.Transform!=null)
                    return;
                _transform.Destroy();
                _type = value;         
                var primitive =GameObject.CreatePrimitive(Type);
                primitive.name = Name;
                _transform.Transform = primitive.transform;
            }
            get => _type;
        }

        [Key(202)]
        public override bool CanCollide
        {
            get => base.CanCollide;
            set
            {
                if (_collider!=null)
                    UnityEngine.Object.Destroy(_collider);
                if (_cancollide == value)
                    return;
                _cancollide = value;
                if (!_cancollide)
                    return;
                switch (_type)
                {
                    case PrimitiveType.Cube:
                        _collider = _transform.Transform.gameObject.AddComponent<BoxCollider>();
                        eosColliderAdaptor.RegistCollisionEvent(this, null);
                        break;
                }
            }
        }
        [IgnoreMember]public int PType
        {
            set
            {
                Type = (PrimitiveType) value;
            }
        }
        public EosShape()
        {

        }
        public EosShape(PrimitiveType type)
        {
            Type = type;
        }
        public override void OnCreate()
        {
            _children = _children ?? new List<EosObjectBase>();
            Type = _type;
            _transform.Transform.gameObject.layer = _layer;
            if (!_visible)
                ApplyVisible();
        }
    }
}

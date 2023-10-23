using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    using Ore;
    [EosObject]
    [System.Serializable]
    public partial class EosMeshObject : EosTransformActor
    {
        [IgnoreMember]public MeshOre Mesh;
        [RequireMold("MeshMold")]
        [Inspector("Ore", "Mesh")]
        [Key(331)] public OreReference MeshOre { get; set; }
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosMeshObject targetmesh))
                return;
            targetmesh.Mesh = Mesh;
            base.OnCopyTo(target);
        }

        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            var mesh = Mesh?.Instantiate();
            if (_transform!=null)
            {
                mesh.parent = _transform.Transform.parent;
                mesh.localPosition = _transform.LocalPosition;
                mesh.localRotation = _transform.Transform.localRotation;
                GameObject.Destroy(_transform.Transform.gameObject);
            }
            mesh.name = Name;
            _transform.Transform = mesh;
        }
    }
}
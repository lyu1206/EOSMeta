using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    using Ore;
    [System.Serializable]
    [EosObject]
    public class EosTool : EosObjectBase
    {
        private bool _isattached;
        [Inspector("Grip","Bone")]
        [Key(331)] public string GripBone { get; set; } = "Bip001 R Hand";
        [Inspector("Grip", "Pos")]
        [Key(332)] public Vector3 GripPos { get; set; } = new Vector3(-1, 0.6f, 0);
        [Inspector("Grip", "Rot")]
        [Key(333)] public Vector3 GripRot { get; set; } = new Vector3(0, -90, 90);
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosTool targettool))
                return;
            targettool.GripBone = GripBone;
            targettool.GripPos = GripPos;
            targettool.GripRot = GripRot;
            base.OnCopyTo(target);
        }
        private void Attach(EosPawnActor topart)
        {
            if (_isattached)
                return;
            
            var part = FindChild<EosMeshObject>();
            var totransform = topart.Transform.Transform.FindDeepChild(GripBone);
            _isattached = true;
            part.Transform.Transform.SetParent(totransform);
            part.Transform.LocalPosition = GripPos;
            part.Transform.LocalRotation = GripRot;
        }
        private void Detach()
        {
            if (!_isattached)
                return;
            _isattached = false;
        }
        public override void OnAncestryChanged()
        {
            var topart = _parent.FindChild<EosPawnActor>();
            if (topart==null)
                return;
            Attach(topart);
        }
    }
}
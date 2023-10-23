using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    using Service;
    using Ore;
    [System.Serializable]
    [EosObject]
    [DescendantOf(typeof(GUIService))]
    public partial class EosUIObject : EosTransformActor
    {
        [IgnoreMember]public OreBase _uisource;
        [RequireMold("UIMold")]
        [Inspector("Ore","UIOre")]
        [Key(331)]
        public OreReference UIOre { get; set; }
        protected override void OnActivate(bool active)
        {
            if (Transform.HasChild)
                return;
            var ui = UIOre.GetOre();
            _uisource = ui.GetComponent<OreBase>();
            if (_uisource == null)
                return;
            _uisource.Instantiate(_transform.Transform);
        }
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosUIObject targetui))
                return;
            targetui._uisource = _uisource;
            base.OnCopyTo(target);
        }
        public override void OnAncestryChanged()
        {
            var lp = LocalPosition;
            var lr = LocalRotation;
            base.OnAncestryChanged();
                        LocalPosition = lp;
                        LocalRotation = lr;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    using Ore;
    [EosObject]
    [System.Serializable]
    public partial class EosLight : EosTransformActor
    {
        // [IgnoreMember]public OreBase Light;
        // [IgnoreMember]public LightController _lightcontroller;
        // [RequireMold("LightMold")]
        // [Inspector("Ore", "Light")]
        // [Key(101)] public OreReference LightOre { get; set; }
        // protected override void OnActivate(bool active)
        // {
        //     if (!ActiveInHierachy)
        //         return;
        //     _lightcontroller = _transform.Transform.GetComponentInChildren<LightController>();
        //     if (_lightcontroller==null)
        //         BuildLight();
        // }
        // private void BuildLight()
        // {
        //     var lightore = LightOre.GetOre();
        //     Light = lightore.GetComponent<OreBase>();
        //     if (Light == null)
        //         return;
        //     var light = Light.Instantiate();
        //     light.transform.parent = _transform.Transform;
        //     light.gameObject.SetActive(true);
        //     _lightcontroller = light.GetComponent<LightController>();
        // }
        // public override void OnCopyTo(EosObjectBase target)
        // {
        //     if (!(target is EosLight targetlight))
        //         return;
        //     targetlight.Light = Light;
        //     base.OnCopyTo(target);
        // }
    }
}
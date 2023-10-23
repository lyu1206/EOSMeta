using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    using Ore;
    using Service;
    [System.Serializable]
    [EosObject]
    [DescendantOf(typeof(TerrainService))]
    public partial class EosTerrain : EosObjectBase
    {
        [RequireMold("TerrainMold")]
        [Inspector("Terrain", "Ore")]
        [Key(41)] public Ore.OreReference TerrainOre { get; set; } = new OreReference();
        protected override void OnActivate(bool active)
        {
            if (!(_parent is TerrainService ts))
                return;
            var terrain = TerrainOre.Instantiate();
            if (terrain == null)
                return;
            terrain.transform.SetParent(ts.Transform.Transform, true);
            var pvm = terrain.GetComponent<PVMOre>();
            pvm.Instantiate(ts.Transform.Transform);
        }
    }
}
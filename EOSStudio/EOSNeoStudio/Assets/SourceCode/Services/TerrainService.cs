using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Service
{
    using Ore;
    using Eos.Objects;
    [NoCreated]
    [System.Serializable]
    public partial class TerrainService : EosService
    {
        public TerrainService()
        {
            Name = "TerrainService";
        }
        [IgnoreMember] public PVMOre _pvmOre;
        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            _pvmOre?.Instantiate(_transform.Transform);
        } 
        public Transform FindNode(string name)
        {
            return _transform.Transform.FindDeepChild(name);
        }
    }
}
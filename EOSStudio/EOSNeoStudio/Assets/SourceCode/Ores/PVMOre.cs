using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Eos.Ore
{
    public class PVMOre : OreBase
    {
        [SerializeField]
        private GameObject _background;
        [SerializeField]
        private Object _navmeshdata;
        public override Transform Instantiate(Transform root)
        {
            var map = GameObject.Instantiate(_background) as GameObject;
            map.transform.parent = root;
            map.transform.localPosition = _background.transform.localPosition;
            map.transform.localRotation = _background.transform.localRotation;
            map.transform.localScale = _background.transform.localScale;
            NavMesh.AddNavMeshData(_navmeshdata as NavMeshData);
            return map.transform;
        }
    }
}
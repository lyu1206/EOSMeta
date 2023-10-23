using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Ore
{
    public partial class BodyOre : OreBase
    {
        [SerializeField]
        private GameObject _body;
        [SerializeField]
        private GameObject[] _initialparts;
    }
}
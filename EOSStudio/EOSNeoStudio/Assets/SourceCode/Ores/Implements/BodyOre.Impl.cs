using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Ore
{
    public partial class BodyOre
    {
        public GameObject GetBody()
        {
            if (_body==null)
                return null;
            return GameObject.Instantiate(_body) as GameObject;
        }
        public GameObject []GetInitialParts()
        {
            if (_initialparts==null)
                return null;
            return _initialparts;
        }
    }
}
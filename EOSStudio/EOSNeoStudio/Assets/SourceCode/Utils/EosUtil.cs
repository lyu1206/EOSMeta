using System.Collections;
using System.Collections.Generic;
using Eos.Objects;
using UnityEngine;
using System.Linq;

namespace EosUtil
{
    public static class Math
    {
        public static Vector3 GetSomething(Vector3 center,Vector3 F,Vector3 N,float factor)
        {
            var Fp = center + F;
            var Np = center + N;
            var cdistance = (Fp - Np).magnitude;
            cdistance = Mathf.Min(cdistance, factor);
            var scale = factor / cdistance;
            var angle = Vector3.Angle(F, N);
            var targetangle = angle * scale;
            var nv = Quaternion.Euler(0,targetangle,0) * N;
            return nv;
        }
    }
}

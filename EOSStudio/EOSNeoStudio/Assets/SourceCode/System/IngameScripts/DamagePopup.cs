using Eos.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Script
{

    [InGameScript]

    public class DamagePopup : IScript
    {
        public EosObjectBase script { get; set; }
        public bool Enable
        {
            set
            {
            }
        }

        public IEnumerator Body()
        {
            var eostrans = script.Parent as EosTransformActor;
            eostrans.Parent = script.Ref.Solution.Workspace;
            eostrans.Transform.LocalScale = Vector3.one * 3;
            while(eostrans.Transform.LocalScale.x>0)
            {
                eostrans.LocalPosition += Vector3.up * Time.deltaTime * 20.5f;
                eostrans.Transform.LocalScale -= Vector3.one * Time.deltaTime * 8;
                yield return new WaitForEndOfFrame();
            }
            eostrans.Destroy();
            yield return 0;
        }

        public void Finish()
        {
        }

        public void Stop()
        {
        }
    }
}
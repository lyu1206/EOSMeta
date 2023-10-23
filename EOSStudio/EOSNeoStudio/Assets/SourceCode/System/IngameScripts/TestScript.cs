using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Eos.Objects;
using Eos.Script;

namespace Eos.Script
{
    [InGameScript]
    public class TestScript : Eos.Script.IScript
    {
        public EosObjectBase script{get;set;}
        public bool Enable
        {
            set
            {
            }
        }
        public IEnumerator Body()
        {
            int count = 3;
            while(count>0)
            {
                Debug.Log($"script test {count} - name:{script.Parent.Name}");
                yield return new WaitForSeconds(3);
                count--;
            }
            yield return 0;
        }

        public void Finish()
        {
            Debug.Log("Test finished");
        }

        public void Stop()
        {
            Debug.Log("Test SCript Stoppwes.");
        }
    }
}
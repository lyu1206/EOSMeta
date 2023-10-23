using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Eos.Objects;
using Eos.Script;

namespace Eos.Script
{

    [InGameScript]
    public class HostileNPC : Eos.Script.IScript
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
            var aiservice = script.Ref.Solution.AIService;

            var humanoid = script.Parent.FindChild<EosHumanoid>();
            humanoid.Speed = 30;

            yield return new WaitCondition(()=>humanoid.Humanoidroot != null);
            humanoid.AIStart();

            var attackrange = humanoid.SetupAttackRange(10,5);
            // var fsm = humanoid.FSM;
            // attackrange.OnTriggerEnter += (sender, collidee) =>
            // {
            //     var adapter = collidee.GetComponent<eosColliderAdaptor>();
            //     if (adapter == null)
            //         return;
            //     var collidedhumanoid = adapter.Actor.Parent.FindChild<EosHumanoid>();
            //     if (collidedhumanoid != humanoid.TraceTarget)
            //         return;
            //     fsm.SetFsmValue("attack", true);
            //     fsm.SetFsmValue("trace", false);
            //     fsm.SetFsmValue("attacktarget", humanoid.TraceTarget);
            //     fsm.FsmTransition("Attack");
            //     attackrange.Enable = false;
            // };

            humanoid.SetupTraceRange(1100);
//            humanoid.SetupTraceRange(10);

            humanoid.OnAIEvent += AttackFinish;

            yield return 0;
        }
        private void AttackFinish(object sender, EosHumanoid.AIEventParameter evinfo)
        {
            var eventname = ((EosHumanoid.AIEventParameter)evinfo).name;
            if (eventname == "attackfinish")
            {
                var humanoid = sender as EosHumanoid;
                humanoid.Ref.Coroutine.OnCoroutineStart(AttackProc(humanoid));
                //Debug.Break();
            }
        }
        private IEnumerator AttackProc(EosHumanoid humanoid)
        {
            yield return new WaitForSeconds(3);
            humanoid.AttackRange.Enable = true;
            humanoid.TraceRange.Enable = true;
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
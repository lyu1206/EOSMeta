using Eos.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Script
{
    using Objects.UI;

    [InGameScript]
    public class EngageLogic : Eos.Script.IScript
    {
        private EosHumanoid _owner;
        public EosObjectBase script { get; set; }
        public bool Enable 
        {
            set
            {
            }
        }

        public IEnumerator Body()
        {
            var humanoid = _owner = script.Parent.FindChild<EosHumanoid>();
            if (humanoid == null)
                yield break;
            yield return new WaitCondition(() => humanoid.Humanoidroot != null);
            humanoid.OnAIEvent += AIEvent;
        }
        private void AIEvent(object sender,EosHumanoid.AIEventParameter parameter)
        {
            var msg = parameter.name;
            if (!(sender is EosHumanoid sendhumanoid))
                return;
            if (msg=="AniEvent")
            {
                if ((string)parameter.args[0] == "hit")
                {
                    var damage = Random.Range(10, 30);
                    //_owner.FSM.SetFsmValue("damage", true);
                    //_owner.FSM.FsmTransition("Damage");
                    var damagepopup = new EosTextMesh("damage");damagepopup.OnCreate();
                    damagepopup.Text = damage.ToString();
                    var popupscript = new EosScript();
                    popupscript.scriptname = "DamagePopup";
                    popupscript.ScriptOre = new Ore.OreReference { Mold = "ScriptMold", OreID = 1 };
                    damagepopup.AddChild(popupscript);
                    _owner.Humanoidroot.AddChild(damagepopup);
                    damagepopup.LocalPosition = Vector3.up * 30;
                    damagepopup.Activate(true);
                }
            }
        }
        public void Finish()
        {
        }

        public void Stop()
        {
        }
    }
}
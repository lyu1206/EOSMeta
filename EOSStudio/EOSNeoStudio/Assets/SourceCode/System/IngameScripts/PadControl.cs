using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Eos.Script
{
    using Objects;
    using Service;
    using EosPlayer;
    [InGameScript]
    public class PadControl : Eos.Script.IScript
    {
        //private UGUIEvents _uguievent;
        private Vector3 _orgposition = Vector3.zero;
        private Vector3 _curposition;
        private RectTransform _padtrans;
        //private EosFsm _playerfsm;
        private EosHumanoid _playerhumanoid;

        public EosObjectBase script { get; set; }
        public bool Enable
        {
            set
            {
                //if (!value)
                //    script.Ref.Solution.GUIService.UnRegistUIEvent(_uguievent);
            }
        }
        private void OnBeginDrag(object sender, PointerEventData data)
        {
            //_playerfsm.SetFsmValue("move", true);
            //_playerfsm.FsmTransition("Move");
        }
        private void OnDragging(object sender, PointerEventData data)
        {
            _curposition += (Vector3)data.delta;
            var delta = _curposition - _orgposition;
            var deltadistance = delta.magnitude;
            delta.Normalize();
            deltadistance = Mathf.Min(deltadistance, 40);


            var direction = EosCamera.Main.CameraspaceDirection(new Vector3(delta.x, 0, delta.y));
            direction.y = 0;
            direction.Normalize();
            var curdirection = _playerhumanoid.Humanoidroot.Transform.Transform.forward;
            float dot = Vector3.Dot(curdirection , direction);
            if (dot<0)
            {
                //_playerfsm.SetFsmValue("attack", false);
                //_playerfsm.SetFsmValue("damage", false);
                //_playerfsm.SetFsmValue("stopattack", true);
                //_playerfsm.FsmTransition("Move");
                //_playerfsm.SetFsmValue("stopattack", false);
            }
            _padtrans.anchoredPosition = _orgposition + delta * deltadistance;
            //_playerfsm.SetFsmValue("movedirection", direction);
        }
        private void OnEndDragging(object sender, PointerEventData data)
        {
            _padtrans.anchoredPosition = _orgposition;
            _curposition = _orgposition;
            //_playerfsm.SetFsmValue("move", false);
            //_playerfsm.SetFsmValue("attack", false);
            //_playerfsm.SetFsmValue("stopattack", true);
            //_playerfsm.FsmTransition("StopMove");
            //_playerfsm.SetFsmValue("stopattack", false);
        }
        public IEnumerator Body()
        {
            if (!(script.Parent is EosTransformActor uiobj))
                yield break;
            yield return new WaitCondition(() => script.Ref.Solution.Players.LocalPlayer != null);
            yield return new WaitCondition(() => script.Ref.Solution.Players.LocalPlayer.Humanoid != null);
            _playerhumanoid = script.Ref.Solution.Players.LocalPlayer.Humanoid;
            var player =  script.Ref.Solution.Players.FindChild<Player>();
            //_playerfsm = player.FindDeepChild<EosFsm>();

            //var controll = uiobj.Transform.Transform.FindDeepChild("PadControl");
            //_padtrans = controll.GetComponent<RectTransform>();
            //var uguievent = _uguievent = script.Ref.Solution.GUIService.RegistUIEvent(controll, this);
            //uguievent.RegistOnBeginDrag(OnBeginDrag);
            //uguievent.RegistOnDrag(OnDragging);
            //uguievent.RegistOnEndDrag(OnEndDragging);
        }

        public void Stop()
        {
            //_uguievent.UnRegistOnBeginDrag(OnBeginDrag);
            //_uguievent.UnRegistOnDrag(OnDragging);
            //_uguievent.UnRegistOnEndDrag(OnEndDragging);
            script.Ref.ScriptPlayer.UnRegistScript(this);
        }

        public void Finish()
        {
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Ore
{
    using Objects;
    public class BoltHumanoid : BoltLinkOre
    {
        public EosPawnActor _humanoidroot;
        private EosHumanoid _humanoid;
        public override void SetObject(EosObjectBase eosobj)
        {
            if (!(eosobj is EosHumanoid humanoid))
                return;
            _humanoid = humanoid;
            _humanoidroot = humanoid.Humanoidroot;
            _humanoidroot.OnAnimationStopped += AnimationDone;
        }
        public Vector3 Position
        {
            get=>_humanoidroot.Transform.LocalPosition;
            set
            {
                _humanoidroot.Transform.LocalPosition = value;
            }
        }
        public Vector3 Rotation
        {
            get=>_humanoidroot.Transform.LocalRotation;
            set
            {
                _humanoidroot.Transform.LocalRotation = value;
            }
        }
        public Vector3 MoveDirection
        {
            set
            {
                _humanoid.MoveDirection = value;
            }
        }
        public bool IsStop => _humanoid.IsStop;
        public void MoveTo()
        {
            var slimemodel = _humanoid.Ref.Solution.Workspace.FindChild<EosModel>("Slime0");
            var slime = slimemodel.FindChild<EosPawnActor>();
            _humanoid.MoveTo(slime.LocalPosition);
        }
        public void MoveTo(Vector3 dest)
        {
            _humanoid.MoveTo(dest);
        }
        public void MoveTo(Transform dest)
        {
            MoveTo(dest.transform.position);
        }
        public void MoveTo(EosHumanoid dest)
        {
            var accesspoint = dest.Ref.Solution.AIService.GetAccesPoint(_humanoid, dest);
            MoveTo(accesspoint);
        }
        private int lookatupdator = -1;
        public void LookAt(EosHumanoid dest)
        {
            if (lookatupdator != -1)
                _humanoid.Ref.Scheduler.UnSchedule(lookatupdator);
            float factor = 0;
            var target = dest.Humanoidroot.LocalPosition;
            var startrot = _humanoid.Humanoidroot.Transform.Transform.localRotation;
            var direction = target - _humanoid.LocalPosition;
            var targetrot = Quaternion.LookRotation(direction.normalized);
            lookatupdator = _humanoid.Ref.Scheduler.ScheduleOnCondition(() =>
            {
                factor += Time.deltaTime * 5;
                var destrot = Quaternion.Lerp(startrot, targetrot, factor);
                _humanoid.Humanoidroot.Transform.Transform.localRotation = destrot;
                factor = Mathf.Min(1, factor);
                _humanoid.UpdateHumanoidPosition();
            },
            () =>
            {
                return factor >= 1;
            });
        }
        public void Stop()
        {
            _humanoid?.Stop();
        }
        public void PlayNode(string name)
        {
            _humanoidroot?.PlayNode(name);
        }
        public void RePlayNode(string name)
        {
            _humanoidroot?.PlayNode(name,true);
        }
        public bool HaveAttackTarget()
        {
            var target = _humanoid.Ref.Solution.AIService.GetAttackbleTarget(_humanoid);
            return target != null;
        }
        public void AnimationDone(object sender,string name)
        {
            // CustomEvent.Trigger(gameObject, "anidone", name);
        }
        public void AIEvent(string name_,params object []args_)
        {
            _humanoid.OnAIEvent?.Invoke(_humanoid, new EosHumanoid.AIEventParameter { name = name_, args = args_ });
        }
        public void HumanoidEvent(EosHumanoid target,string msg,object arg)
        {
            target.OnAIEvent?.Invoke(_humanoid, new EosHumanoid.AIEventParameter { name = msg, args = new object[] { arg } });
        }
        private void OnDestroy()
        {
            _humanoidroot.OnAnimationStopped -= AnimationDone;
        }
    }
}
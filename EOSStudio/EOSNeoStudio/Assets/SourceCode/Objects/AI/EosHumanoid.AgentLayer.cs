using System.Collections;
using System.Collections.Generic;
using Eos.Objects;
using UnityEngine;

namespace Eos.Objects
{
    public partial class EosHumanoid
    {
        public class EosHumanoidAgentLayer
        {
            private Dictionary<int, string> _behaviorAnimations = new Dictionary<int, string>();
            private List<IMoveAgentBehavior> _activeBehaviors = new List<IMoveAgentBehavior>();
            public List<IMoveAgentBehavior> ActiveBehaviors => _activeBehaviors;

            public virtual void OnEnter(EosHumanoid entity, EosHumanoidAgentLayer currentlayer)
            {
            }

            public virtual void OnEnd(EosHumanoid entity)
            {
            }

            public virtual void OnUpdate(EosHumanoid entity, float delta)
            {
            }

            public void SetBehaviorAnimation(MoveAgentState state, string animation)
            {
                _behaviorAnimations[(int) state] = animation;
            }

            public void PlayAnimation(EosPawnActor actor, MoveAgentState state)
            {
                Debug.Log($"PlayBehaviorAnimation : {_behaviorAnimations[(int) state]}");
                
                actor?.PlayNode(_behaviorAnimations[(int) state]);
            }
        }

        public class EosHumanoidRunLayer : EosHumanoidAgentLayer
        {
            public override void OnEnter(EosHumanoid entity, EosHumanoidAgentLayer currentlayer)
            {
                base.OnEnter(entity, currentlayer);
            }

            public override void OnEnd(EosHumanoid entity)
            {
                base.OnEnd(entity);
            }
        }

        public class EosHumanoidSwimLayer : EosHumanoidAgentLayer
        {
            public override void OnEnter(EosHumanoid entity, EosHumanoidAgentLayer currentlayer)
            {
                base.OnEnter(entity, currentlayer);
            }

            public override void OnEnd(EosHumanoid entity)
            {
                base.OnEnd(entity);
            }
        }

        public class EosHumanoidFlyLayer : EosHumanoidAgentLayer
        {
            public override void OnEnter(EosHumanoid entity, EosHumanoidAgentLayer currentlayer)
            {
                base.OnEnter(entity, currentlayer);
            }

            public override void OnEnd(EosHumanoid entity)
            {
                base.OnEnd(entity);
            }
        }

        public class EosHumanoidRideLayer : EosHumanoidAgentLayer
        {
            public override void OnEnter(EosHumanoid entity, EosHumanoidAgentLayer currentlayer)
            {
                base.OnEnter(entity, currentlayer);
            }

            public override void OnEnd(EosHumanoid entity)
            {
                base.OnEnd(entity);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace Eos.Objects
{
    public enum PropertyIndex
    {
        MoveDirection,
    }
    public partial class EosHumanoid
    {
        private static event EventHandler<Vector3> _movedirectionchanged;
        private static event EventHandler _jumpevent;

        private Dictionary<int, IMoveAgentBehavior> _behaviors;
        private Dictionary<int,EosHumanoidAgentLayer> _humanoidAgentLayers = new Dictionary<int,EosHumanoidAgentLayer>(); 
        private Dictionary<int,Type> _humanoidAgentLayersTypes= new Dictionary<int, Type>();
        private IMoveAgentBehavior _currentBehavior;

        private int _moveframedelay = 0;
        private bool _onground;
        private IMoveAgentBehavior CurrentState => _currentAgentLayer.ActiveBehaviors[_currentAgentLayer.ActiveBehaviors.Count - 1];

        private void InitMoveAgentBehaviros()
        {
            if (_behaviors != null)
                return;
            _behaviors = _moveagent.InitBehaviros();
            _humanoidAgentLayersTypes[(int)MoveAgentState.Move] = typeof(EosHumanoidRunLayer);
            _humanoidAgentLayersTypes[(int)MoveAgentState.Swim] = typeof(EosHumanoidSwimLayer);
            _humanoidAgentLayersTypes[(int)MoveAgentState.Fly] = typeof(EosHumanoidFlyLayer);
            _humanoidAgentLayersTypes[(int)MoveAgentState.Ride] = typeof(EosHumanoidRideLayer);
            
            foreach (var it in _behaviors)
                it.Value.OnAwake(this);
            RegistComponent(BehaviorUpdate);
            
            SetAgentLayer(MoveAgentState.Move);
            SetBehaviorAnimation(MoveAgentState.Idle,"Stand");
            SetBehaviorAnimation(MoveAgentState.Move,"Run");
            SetBehaviorAnimation(MoveAgentState.Jump,"Jump");
            SetBehaviorAnimation(MoveAgentState.Fall,"Jump");
            SetBehaviorAnimation(MoveAgentState.Die,"Die");
            
            SetAgentLayer(_currentAgentState);
        }

        private void SetAgentLayer(MoveAgentState layer)
        {
            if (!_humanoidAgentLayers.ContainsKey((int) layer))
                _humanoidAgentLayers[(int) layer] = Activator.CreateInstance(_humanoidAgentLayersTypes[(int) layer]) as EosHumanoidAgentLayer;
            if (_currentAgentLayer == _humanoidAgentLayers[(int) layer])
                return;
            _currentAgentLayer?.OnEnd(this);
            var nextlayer = _humanoidAgentLayers[(int) layer];
            nextlayer.OnEnter(this,_currentAgentLayer);
            _currentAgentLayer = nextlayer;
            _humanoidactionlayer = (int)layer; 
//            NotifyPropertyChange(EosPropertyNotify.HumanoidActionLayer);
        }
        private void SetBehaviorAnimation(MoveAgentState state, string animation)
        {
            _currentAgentLayer.SetBehaviorAnimation(state, animation);
        }

        private void PlayBehaviorAnimation(MoveAgentState state)
        {
            if (!NotifyOut)
                return;
            _currentAgentLayer.PlayAnimation(_humanoidroot,state);
        }
        private void ActiveBehavior(MoveAgentState state)
        {
            if (_behaviors == null)
                return;
            if (!_behaviors.ContainsKey((int)state))
                return;
            ActiveBehavior(_behaviors[(int)state]);
        }
        private void BehaviorUpdate(object sender, float delta)
        {
            var activebehaviros = new List<IMoveAgentBehavior>(_currentAgentLayer.ActiveBehaviors);
            activebehaviros.ForEach(it => it.OnUpdate(this, delta));
        }

        private void StartBehavior(IMoveAgentBehavior behavior)
        {
            // Debug.Log($"State changed:{behavior.State}");
            _currentBehavior = behavior;
            _humanoidBehavior = (int)behavior.State;
            behavior.OnStart(this);
            PlayBehaviorAnimation(behavior.State);
            //NotifyPropertyChange(EosPropertyNotify.HumanoidBehavior);
        }
        private void ActiveBehavior(IMoveAgentBehavior behavior)
        {
            var activeBehaviors = _currentAgentLayer.ActiveBehaviors;
            if (activeBehaviors.Contains(behavior))
                return;
            if (activeBehaviors.Count == 0)
            {
                activeBehaviors.Add(behavior);
                StartBehavior(behavior);
            }
            else
            {
                bool added = false;
                var layer = behavior.Priority;
                for (int i = 0; i < activeBehaviors.Count; i++)
                {
                    if (activeBehaviors[i].Priority == layer)
                    {
                        activeBehaviors[i].OnEnd(this);
                        _currentAgentLayer.ActiveBehaviors.Remove(activeBehaviors[i]);

                    }
                }
                for (int i = 0; i < activeBehaviors.Count; i++)
                {
                    var it = activeBehaviors[i];
                    if (!it.AllowMultiState)
                    {
                        it.OnEnd(this);
                        activeBehaviors.RemoveAt(i);
                        i--;
                    }
                }
                for (int i=0;i<activeBehaviors.Count;i++)
                {
                    var it = activeBehaviors[i];
                    if (it.Priority > behavior.Priority)
                    {
                        activeBehaviors.Insert(i,behavior);
                        added = true;
                        break;
                    }
                }

                if (!added)// when add highest priority behaivor 
                {
                    activeBehaviors.Add(behavior);
                    StartBehavior(behavior);
                }
            }
        }
        private void InActiveBehavior(IMoveAgentBehavior behavior)
        {
            behavior.OnEnd(this);
            var activeBehaviors = _currentAgentLayer.ActiveBehaviors;
            if (activeBehaviors==null || activeBehaviors.Count==0)
                return;
            var currentbehavior = activeBehaviors[activeBehaviors.Count - 1];
            activeBehaviors.Remove(behavior);
            if (activeBehaviors.Count == 0)
                ActiveBehavior(MoveAgentState.Idle);
            else
            {
                int highpriority = -1;
                IMoveAgentBehavior nextbehavior = null;
                foreach(var it in activeBehaviors)
                {
                    if (highpriority < it.Priority)
                    {
                        nextbehavior = it;
                        highpriority = it.Priority;
                    }
                }
                if (nextbehavior != null && currentbehavior.Priority!=nextbehavior.Priority)
                    StartBehavior(nextbehavior);
            }
        }
        private void RegistMoveDirectionChangedEvent(EventHandler<Vector3> handle)
        {
            _movedirectionchanged += handle;
        }

        private void TestFalling(object sender, object[]args)
        {
            var root = _humanoidroot;
            if (_jump)
                return;
            if (root.Rigidbody.velocity.y>0)
                return;
            var collision = (Collision)args[0];
            if (collision.gameObject.layer != 9)
                return;
            ActiveBehavior(MoveAgentState.Fall);
            _onground = _fall;
            Debug.Log("///////////// Falling ..");
        }
        private void TestLandingWithVelocity(object sender,float delta)
        {
            if (_humanoidroot.Rigidbody.velocity.y >= 0)
            {
                InActiveBehavior(_currentBehavior);
            }
        }
        private void TestLanding(object sender,object []args)
        {
            if (!(sender is EosTransformActor owner))
                return;
            var humanoid = owner.Parent.FindChild<EosHumanoid>();
            if (humanoid==null)
                return;
            var data = (Collision)args[0];
            if (data.gameObject.layer !=9)
                return;
            if (data.contactCount == 1 && data.contacts[0].normal == Vector3.up)
            {
                _onground = true;
                humanoid.InActiveBehavior(_currentBehavior);                    
                return;
            }
            else
            {
                if (Math.Abs(data.contacts[0].point.y - owner.WorldPosition.y) < 0.1f)
                {
                    _onground = true;
                    humanoid.InActiveBehavior(_currentBehavior);
                    return;
                }
                Debug.Log($"Jump not collieded Vector.up:{data.contacts[0].normal}");
                humanoid._moveblocked = true;
            }

            // Debug.Log($"///////////// Landing .. : {humanoid.Humanoidroot.Rigidbody.velocity}");
        }
        
        private void TestFallingDown(object sender,float delta)
        {
            if (_fall)
                return;
            if (_humanoidroot.Rigidbody == null)
                return;
           //return _humanoidroot.Collders[0].Collider.RayCast();
           //Debug.Log($"Test falling:{_humanoidroot.Rigidbody.velocity}");
           var isfalling = _humanoidroot.Rigidbody.velocity.y < -0.02f;
           if (isfalling)
           {
               Debug.Log($"falling....");
               ActiveBehavior(MoveAgentState.Fall);
           }
        }
        public interface IMoveAgent
        {
            float Angularspeed { set; }
            float Speed { set; }
            float Accelation { set; }
            void MoveDirection(EosHumanoid entity, Vector3 direction);
            Dictionary<int, IMoveAgentBehavior> InitBehaviros();

        }
        public interface IMoveAgentBehavior
        {
            int Priority { get; }
            bool AllowMultiState { get; }
            MoveAgentState State { get; }
            void OnAwake(EosHumanoid entity);
            void OnStart(EosHumanoid entity);
            void OnEnd(EosHumanoid entity);
            void OnUpdate(EosHumanoid entity, float delta);
        }
        public class NavigationAgent : IMoveAgent
        {
            private NavMeshAgent _navagent;
            private static Dictionary<int, IMoveAgentBehavior> _behaviors;
            public float Angularspeed{set => _navagent.angularSpeed = value;}
            public float Speed { set => _navagent.speed = value; }
            public float Accelation { set => _navagent.acceleration = value; }
            public void MoveDirection(EosHumanoid entity, Vector3 direction)
            {
            }
            public Dictionary<int, IMoveAgentBehavior> InitBehaviros()
            {
                return null;
            }
        }
        public class PhysicsAgent : IMoveAgent
        {
            private static Dictionary<int, IMoveAgentBehavior> _behaviors;
            private float _angularSpeed = 1000;
            private float _speed = 0.5f;
            private float _acceleration = 35;
            private bool _moveblocked = false;
            public float Angularspeed {set => _angularSpeed = value; }
            public float Speed { set => _speed = value; }
            public float Accelation { set => _acceleration = value; }
            public Dictionary<int, IMoveAgentBehavior> InitBehaviros()
            {
                if (_behaviors != null)
                    return _behaviors;
                var behaviros = new Dictionary<int, IMoveAgentBehavior>();
                behaviros[(int)MoveAgentState.Idle] = new Idle();
                behaviros[(int)MoveAgentState.Move] = new Move();
                behaviros[(int)MoveAgentState.Jump] = new Jump();
                behaviros[(int)MoveAgentState.Fall] = new Fall();
                behaviros[(int)MoveAgentState.Die] = new Die();
                _behaviors = behaviros;
                return behaviros;
            }
            public void MoveDirection(EosHumanoid entity, Vector3 direction)
            {
                var root = entity._humanoidroot;
                var newdirection = direction.normalized;
                // if (entity._movedirection == newdirection)
                //     return;
                entity._movedirection = newdirection;
                EosHumanoid._movedirectionchanged?.Invoke(entity, newdirection);
            }
            #region States
            public class Idle : IMoveAgentBehavior
            {
                public int Priority => 0;

                public bool AllowMultiState => false;
                public MoveAgentState State => MoveAgentState.Idle;

                public void OnAwake(EosHumanoid entity) { }
                public void OnStart(EosHumanoid entity)
                {
                    var velocity = Vector3.zero;
                    var root = entity._humanoidroot;
                    if (root.Rigidbody != null)
                    {
                        entity._humanoidroot.Rigidbody.velocity = velocity;
                        entity._humanoidroot.Rigidbody.constraints =
                            RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    }

                    entity._moveframedelay = 1;
                    if (!(root is IColliderEvent colliderEvent))
                        return;
                    colliderEvent.OnCollisionExit.Connect(entity.TestFalling);
                    
//                    entity.RegistComponent(entity.TestFallingDown);
                }
               
                public void OnEnd(EosHumanoid entity)
                {
  //                  entity.UnRegistComponent(entity.TestFallingDown);
                    // var root = entity._humanoidroot;
                    // root.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                    // if (!(root is IColliderEvent colliderEvent))
                    //     return;
                    // colliderEvent.OnCollisionExit.DisConnect(entity.TestFalling);
                }


                public void OnUpdate(EosHumanoid entity, float delta)
                {
                    if (entity._moveframedelay != 0)
                    {
                        entity._moveframedelay = 0;
                        return;
                    }
                    if (entity.CurrentState !=this)
                        return;
                    // var falling = entity.TestFallingDown();
                    // if (falling)
                    // {
                    //     entity.ActiveBehavior(MoveAgentState.Fall);
                    // }
                }
            }

            public class Move : IMoveAgentBehavior , INotifyObject
            {
                public int Priority => 1;
                public bool AllowMultiState => true;

                public MoveAgentState State => MoveAgentState.Move;

                public void OnAwake(EosHumanoid entity)
                {
                    entity.AddObjectToNotify(this);                    
                    // _movedirectionchanged += MoveDirectionChanged;
                }
                public void OnNotifyProperyChanged(EosNotifyObject sender, EosPropertyNotify notify,bool incomming)
                {
                    if (!(sender is EosHumanoid humanoid))
                        return;
                    if (notify == EosPropertyNotify.HumanoidMoveDirection)
                        MoveDirectionChanged(sender,humanoid._movedirection);
                }

                public void OnNotifyObjectAction(EosNotifyObject sender, EosObjectAction action,bool incomming)
                {
                }
                private void MoveDirectionChanged(object sender,Vector3 direction)
                {
                    if (!(sender is EosHumanoid humanoid))
                        return;
                    if (direction == Vector3.zero)
                        humanoid.InActiveBehavior(this);
                    else
                        humanoid.ActiveBehavior(this);
                }

                public void OnStart(EosHumanoid entity)
                {
                    entity._moveframedelay = 1;
                    var root = entity._humanoidroot;
                    if (!(root is IColliderEvent colliderEvent))
                        return;
                    colliderEvent.OnCollisionExit.Connect(entity.TestFalling);
                }
                public void OnEnd(EosHumanoid entity)
                {
                    // var velocity = entity._humanoidroot.Rigidbody.velocity;
                    // var root = entity._humanoidroot;
                    // velocity.x = velocity.z = 0;
                    // root.Rigidbody.velocity = velocity;
                    // if (!(root is IColliderEvent colliderEvent))
                    //     return;
                    // colliderEvent.OnCollisionExit.DisConnect(entity.TestFalling);
                }
                public void OnUpdate(EosHumanoid entity, float delta)
                {
                    // var moveagent = entity._moveagent as PhysicsAgent;
                    // var velocity = entity._movedirection * ((!entity._moveblocked)?moveagent._speed:0.0f);
                    // velocity.y =  entity._humanoidroot.Rigidbody.velocity.y;
                    //
                    // entity._humanoidroot.Rigidbody.velocity = velocity;
                     var autorotation = true;
                     if (autorotation && entity._movedirection != Vector3.zero)
                     {
                         var toRotation = Quaternion.LookRotation(entity._movedirection, Vector3.up);
                         entity._humanoidroot._transform.Transform.rotation = Quaternion.RotateTowards(entity._humanoidroot.Transform.Transform.rotation ,toRotation,1024*delta);
                     }
                    // if (entity._moveframedelay != 0)
                    // {
                    //     entity._moveframedelay = 0;
                    //     return;
                    // }
                    // if (entity.CurrentState !=this)
                    //     return;
                }
            }
            public class Jump : IMoveAgentBehavior , INotifyObject
            {
                public int Priority => 2;

                public bool AllowMultiState => true;
                public MoveAgentState State => MoveAgentState.Jump;

                public void OnAwake(EosHumanoid entity)
                {
                    EosHumanoid._jumpevent += event_Jump;
                }

                private void event_Jump(object sender,EventArgs args)
                {
                    if (!(sender is EosHumanoid humanoid))
                        return;
                    humanoid.ActiveBehavior(this);
                }

                public void OnStart(EosHumanoid entity)
                {
                    Debug.Log("Jump Start");
                    // var root = entity._humanoidroot;
                    // var rigidbody = root.Rigidbody;
                    // var velocity = rigidbody.velocity;
                    // velocity.y = 5f;
                    // rigidbody.velocity = velocity;
                    entity._jump = true;
                    entity._updirection = Vector3.up * 10f * EosHumanoid.HUMANOIDSCALE;
                    entity.MoveUpDirection();
                    entity.NotifyPropertyChange(EosPropertyNotify.HumanoidUpDirection);
                    // if (!(root is IColliderEvent colliderEvent))
                    //     return;
                    // colliderEvent.OnCollisionEnter.Connect(entity.TestLanding);
                    entity.AddObjectToNotify(this);
                    Debug.Log("Connect TestLanding");
                }
                public void OnEnd(EosHumanoid entity)
                {
                    var root = entity._humanoidroot;
                    if (!(root is IColliderEvent colliderEvent))
                        return;
                    // colliderEvent.OnCollisionEnter.DisConnect(entity.TestLanding);
                    Debug.Log("DisConnect TestLanding");
                    entity.DeleteObjectToNotify(this);
                    entity._jump = false;
                    entity._moveblocked = false;
                    entity.UnRegistComponent(entity.TestLandingWithVelocity);
                }

                public void OnUpdate(EosHumanoid entity, float delta)
                {
                }

                public void OnNotifyProperyChanged(EosNotifyObject sender, EosPropertyNotify notify,bool incomming)
                {
                }

                public void OnNotifyObjectAction(EosNotifyObject sender, EosObjectAction action,bool incomming)
                {
                    if (!(sender is EosHumanoid obj))
                        return;
                    if (action == EosObjectAction.HumanoidCollided)
                    {
                        obj.RegistComponent(obj.TestLandingWithVelocity);
                    }

                    if (action == EosObjectAction.Landing)
                    {
                        obj.InActiveBehavior(this);
                    }
                }
            }
            public class Fall : IMoveAgentBehavior , INotifyObject
            {
                public int Priority => 2;

                public bool AllowMultiState => true;
                public MoveAgentState State => MoveAgentState.Fall;

                public void OnAwake(EosHumanoid entity) { }

                public void OnStart(EosHumanoid entity)
                {
                    var root = entity._humanoidroot;
                    if (!(root is IColliderEvent colliderEvent))
                        return;
                    colliderEvent.OnCollisionEnter.Connect(entity.TestLanding);
                    entity.AddObjectToNotify(this);
                    Debug.Log("Connect TestLanding");
                    entity._fall = true;
                }
                public void OnEnd(EosHumanoid entity)
                {
                    var root = entity._humanoidroot;
                    if (!(root is IColliderEvent colliderEvent))
                        return;
                    entity.DeleteObjectToNotify(this);
                    colliderEvent.OnCollisionEnter.DisConnect(entity.TestLanding);
                    entity.UnRegistComponent(entity.TestLandingWithVelocity);
                    Debug.Log("DisConnect TestLanding");
                    entity._fall = false;
                    entity._moveblocked = false;
                }

                public void OnUpdate(EosHumanoid entity, float delta)
                {
                    if (!(entity is EosHumanoid humanoid))
                        return;
//                    Debug.Log($"falling velocity :{humanoid.Humanoidroot.Rigidbody.velocity.y}");
                    // if (Math.Abs(humanoid.Humanoidroot.Rigidbody.velocity.y) <0.02f)
                    // {
                    //     Debug.Log("landing on falling");
                    //     humanoid.InActiveBehavior(this);                    
                    // }
                }

                public void OnNotifyProperyChanged(EosNotifyObject sender, EosPropertyNotify notify,bool incomming)
                {
                }

                public void OnNotifyObjectAction(EosNotifyObject sender, EosObjectAction action,bool incomming)
                {
                    if (!(sender is EosHumanoid obj))
                        return;
                    if (action == EosObjectAction.HumanoidCollided)
                    {
                        obj.RegistComponent(obj.TestLandingWithVelocity);
                    }
                }
            }
            public class Die : IMoveAgentBehavior
            {
                public int Priority => 0;
                public bool AllowMultiState => false;

                public MoveAgentState State => MoveAgentState.Die;

                public void OnAwake(EosHumanoid entity) { }
                public void OnEnd(EosHumanoid entity) { }

                public void OnStart(EosHumanoid entity) { }

                public void OnUpdate(EosHumanoid entity, float delta) { }
            }
            #endregion
        }
    }
}
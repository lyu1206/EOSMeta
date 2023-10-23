using System;
using System.Collections;
using System.Collections.Generic;
//using System.Runtime.Remoting.Channels;
using UnityEngine;
using UnityEngine.AI;
namespace Eos.Objects
{
    using Service.AI;
    using MessagePack;
    public enum MoveAgentState
    {
        Idle,
        Move,
        Jump,
        Fall,
        Fly,
        Swim,
        Ride,
        Die,
    }

    [EosObject]
    [MessagePackObject]
    public partial class EosHumanoid : EosTransformActor// EosObjectBase ,ITransform
    {
        public const float HUMANOIDSCALE = 10f;
        [IgnoreMember]public int Level = 1;
        private IMoveAgent _moveagent;
        private CharacterController _characterController;
        private EosHumanoidAgentLayer _currentAgentLayer = null;
        private MoveAgentState _currentAgentState = MoveAgentState.Move;
        private float _angularspeed = 1000;
        private float _speed = 4 * HUMANOIDSCALE;
        private bool _moveblocked = false;
        private float _accelation = 1000;
        #region Notify Methods
        [ NotifyPropertyMethod((int)EosPropertyNotify.HumanoidMoveDirection)]
        private void Notify_MoveDirection()
        {
            if (_moveagent!=null)
            {
                _moveagent.MoveDirection(this, _movedirection);
            }
        }
        [NotifyPropertyMethod((int)EosPropertyNotify.HumanoidJump)]
        private void Notify_Jump()
        {
            _jumpevent?.Invoke(this,EventArgs.Empty);
        }

        [NotifyPropertyMethod((int) EosPropertyNotify.HumanoidBehavior)]
        private void Notify_HumanoidBehavior()
        {
            
        }
        [NotifyPropertyMethod((int) EosPropertyNotify.HumanoidActionLayer)]
        private void Notify_HumanoidActionLayer()
        {
            
        }
        #endregion
        [Key(31)]
        public float Angularspeed 
        {
            get=>_angularspeed;
            set
            {
                _angularspeed = value;
                if (_moveagent!=null)
                    _moveagent.Angularspeed = value;
            }
        }
        [Key(32)]
        public float Speed
        {
            get =>_speed;
            set
            {
                _speed = value;
                if (_moveagent != null)
                    _moveagent.Speed = value;
            }
        }
        [Key(33)]
        public float Accelation
        {
            get=>_accelation;
            set
            {
                _accelation = value;
                if (_moveagent != null)
                    _moveagent.Accelation = value;
            }
        }
        [EosKeyTag(34, (int) EosPropertyNotify.HumanoidUpDirection)]
        private Vector3 _updirection { get; set; }

        [EosKeyTag(35, (int) EosPropertyNotify.HumanoidMoveDirection)]
        private Vector3 _movedirection { get; set; }

        [EosKeyTag(36, (int) EosPropertyNotify.HumanoidBehavior)]
        public int _humanoidBehavior { get; set; }
        [EosKeyTag(37, (int) EosPropertyNotify.HumanoidActionLayer)]
        public int _humanoidactionlayer { get; set; }

        [IgnoreMember]public Vector3 MoveDirection
        {
            set
            {
                var dir = value.normalized;
                if (_movedirection==dir)
                    return;
                _movedirection = dir;
                Notify_MoveDirection();
                _humanoidroot.NotifyPropertyChange(EosPropertyNotify.LocalPosition);
                NotifyPropertyChange(EosPropertyNotify.HumanoidMoveDirection);
                
                //if (_navagent != null)
                //{
                //    UpdateHumanoidPosition();
                //    _transform.Transform.forward = value;
                //    OnMoveStateChanged?.Invoke(this,true);
                //    _navagent.isStopped = false;
                //    _navagent.SetDestination(_navagent.transform.localPosition + value*_radius*2);
                //}
            }
        }
        public void UpdateHumanoidPosition()
        {
            _transform.LocalPosition = _humanoidroot.LocalPosition;
            _transform.Transform.forward = _humanoidroot.Transform.Transform.forward;
        }

        [IgnoreMember]public bool Jump
        {
            get => _jump;
            set
            {
                if (_jump)
                    return;
                //_jumpevent?.Invoke(this,EventArgs.Empty);
                Notify_Jump();
                NotifyPropertyChange(EosPropertyNotify.HumanoidJump);
            }
        }
        [IgnoreMember] public bool IsStop => (!_navagent.pathPending && _navagent.remainingDistance == 0);
        [IgnoreMember] public const string humanoidroot = "HumanoidRoot";
        [Key(332)]public float _radius;
        private NavMeshAgent _navagent;
        private EosPawnActor _humanoidroot;
        [EosNotifyTag((int)EosPropertyNotify.HumanoidJump)]
        private bool _jump { get; set; }
        private bool _fall;
        private AccessPlate _accesplate;
        [IgnoreMember]public NavMeshAgent NavAgent => _navagent;
        [IgnoreMember] public EosPawnActor Humanoidroot => _humanoidroot;
        [IgnoreMember] public AccessPlate AccesPlate => _accesplate;
        [IgnoreMember] public float Radius => _radius;
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosHumanoid targethumanoid))
                return;
            targethumanoid._radius = _radius;
            targethumanoid._angularspeed = _angularspeed;
            targethumanoid._speed = _speed;
            targethumanoid._accelation = _accelation;
            targethumanoid._moveagent = _moveagent;
            base.OnCopyTo(target);
        }

        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            var root = _parent.FindChild<EosPawnActor>(humanoidroot);
            if (root==null)
                return;
            
            _moveagent = new PhysicsAgent();
            InitMoveAgentBehaviros();

            var bone = root.FindDeepChild<EosBone>();
            if (bone != null)
            {
                bone.OnReadyEvent += (sender, args) =>
                {
                    int a;
                    a = 0;
                    ActiveBehavior(MoveAgentState.Idle);
                };
            }
            
            var rootobject = root.Transform.Transform.gameObject;
            _humanoidroot = root;
            _humanoidroot.OnRefreshRenders += (s, a) =>
            {
                //센터 증심, 반지름 가중치 등등을 잘 잡으면 됩니다.유져가 잡있을수 있도록 설정하세요.그럼 바로 물지턱 위치를 잡는다.
                // 위치를 저장하거나,저장후에도 저장하자.인제 물리를 써서 하면 확실히 편할듯하다.
                _characterController.center = a.center;
                _characterController.radius = (a.extents.x > a.extents.z ? a.extents.x : a.extents.z)*0.5f;
                _characterController.height = a.extents.y * 2;
                _characterController.stepOffset = _characterController.height * 0.1f;
            };
            root.OnReadyEvent += (sender, args) =>
            {
                int a;
                a = 0;
            };
            _characterController = _humanoidroot.Transform.Transform.gameObject.AddComponent<CharacterController>();
            eosHumanoidCollisionAdaptor.RegistCollisionEvent(_humanoidroot);

//            _radius = (_humanoidroot.FindChild<EosCollider>().Collider as eosCapsuleCollider).Radius;

//            _navagent = rootobject.AddComponent<NavMeshAgent>();
//            _navagent.radius = _radius;


            Angularspeed = _angularspeed;
            Speed = _speed;
            Accelation = _accelation;
        }
        public override void StartPlay()
        {
            base.StartPlay();
            
            // var zerofriction = new PhysicMaterial();
            // zerofriction.dynamicFriction = zerofriction.staticFriction = 0.0f;
            // zerofriction.frictionCombine = PhysicMaterialCombine.Multiply;
            // _humanoidroot.Collders[0].Collider.Collider.material = zerofriction;
         
        }

        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x,y,z));
        }
        public void SetPosition(Vector3 to)
        {
            UpdateHumanoidPosition();
            if (_navagent != null) _navagent.enabled = false;
            _humanoidroot.Transform.Transform.position = to;
            if (_navagent != null) _navagent.enabled = true;
        }
        public void MoveTo(Vector3 dest)
        {
            UpdateHumanoidPosition();
            OnMoveStateChanged?.Invoke(this, true);
            _navagent.isStopped = false;
            _navagent.SetDestination(dest);
        }

        public void Move(float x, float y, float z)
        {
            _humanoidroot.WorldPosition = new Vector3(x,y,z);
            // _updirection = Vector3.zero;
            // _movedirection = Vector3.zero;
            // _characterController.Move(Vector3.zero);
        }
        public void Stop()
        {
            UpdateHumanoidPosition();
            OnMoveStateChanged?.Invoke(this, false);
            _navagent.ResetPath();
            _navagent.isStopped = true;
        }
        public override void OnChildAdded(EosObjectBase child)
        {
            if (child is AccessPlate plate)
                _accesplate = plate;
            if (child is EosCollider collider)
            {
                var rigidbody = Transform.Transform.gameObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
            }
        }
        public override void OnChildRemoved(EosObjectBase child)
        {
            if (child is AccessPlate plate)
                _accesplate = null;
        }
        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            if (!(_parent is ITransform transactor))
                return;
            _transform.SetParent(transactor.Transform);
        }
        public override string ToString()
        {
            return $"{_parent.Name}'s humanoid";
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Humanoidroot?.Destroy();
        }

        public void MoveUpDirection()
        {
            _characterController.Move(_updirection * Time.deltaTime);
        }
        public override void Update(float delta)
        {
            base.Update(delta);
            // 캐릭터에 중력 적용.
            
            // if (!_characterController.isGrounded)
            //     _characterMoveDirection.y -= 23 * 10 * Time.deltaTime;
 
            // 캐릭터 움직임.
            bool grounded = _characterController.isGrounded;
            if (_movedirection!=Vector3.zero)
                _characterController.Move(_movedirection *_speed * Time.deltaTime);
            if (!_characterController.isGrounded /*|| _updirection.y != 0*/)
            {
                _updirection -= Vector3.up * 23 * 10 * Time.deltaTime;
                MoveUpDirection();
            }

            if (_characterController.isGrounded && _updirection.y < 0)
            {
                _updirection = Vector3.zero;
                OnObjectAction(EosObjectAction.Landing);
            }
        }
    }
}

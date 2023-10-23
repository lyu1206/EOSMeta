using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//using Battlehub.RTAnimation.Battlehub.SL2;
using UnityEngine;

namespace Eos.Objects
{
    using Signal;
    using Ore;
    using MessagePack;
    public  class  EosSkeleton 
    {
        private Transform[] _bones;
        private readonly Hashtable _bonesByHash =new Hashtable();
        private readonly List<Matrix4x4> _bindposes = new List<Matrix4x4>();
        private Transform _root;
        public void SetupSkeleton(Transform body,Transform root)
        {
            var boneindex = 0;
            _bones = root.GetComponentsInChildren<Transform>(true);
            _root = root.transform;
            foreach (var bone in _bones)
            {
                _bonesByHash.Add(bone.name,boneindex);
                _bindposes.Add(bone.worldToLocalMatrix * body.localToWorldMatrix);
                boneindex++;
            }
        }

        private const int int_loadingTime = 30;
        public async Task SkinedMeshSetup(SkinnedMeshRenderer skinmeshrender,SkinnedMeshRenderer target)
        {
            var newbones = new Transform[skinmeshrender.bones.Length];
            var newbindpos = new Matrix4x4[skinmeshrender.bones.Length];
            for (int i = 0; i < newbones.Length; i++)
            {
                var boneindex = (int) _bonesByHash[skinmeshrender.bones[i].name];
                newbones[i] = _bones[boneindex];
                newbindpos[i] = _bindposes[boneindex];
                // if (i % int_loadingTime == 0)
                //     await Task.Yield();
            }

            var mesh = new Mesh();

            var omesh = skinmeshrender.sharedMesh;
            var bones = skinmeshrender.bones;
            var bonenames = new string[skinmeshrender.bones.Length];
            for (int i = 0; i < skinmeshrender.bones.Length; i++)
            {
                bonenames[i] = skinmeshrender.bones[i].name;
            }
            var ci = new CombineInstance
            {
                mesh =  omesh,
                transform = skinmeshrender.transform.localToWorldMatrix
            };
            mesh.CombineMeshes(new CombineInstance[]{ci});
            mesh.uv = omesh.uv;
            mesh.bindposes = _bindposes.ToArray();

            var boneWeights = new BoneWeight[omesh.boneWeights.Length];
            var omeshLength = omesh.boneWeights.Length;
            var omeshBoneWeights = omesh.boneWeights;
            for (int i = 0; i < omeshLength; i++)
            {
                var bWeight = omeshBoneWeights[i];
                bWeight.boneIndex0 = (int) _bonesByHash[bonenames[bWeight.boneIndex0]];
                bWeight.boneIndex1 = (int) _bonesByHash[bonenames[bWeight.boneIndex1]];
                bWeight.boneIndex2 = (int) _bonesByHash[bonenames[bWeight.boneIndex2]];
                bWeight.boneIndex3 = (int) _bonesByHash[bonenames[bWeight.boneIndex3]];
                boneWeights[i] = bWeight;
                //if (i % int_loadingTime == 0)
                //    await Task.Yield();
            }

            
            mesh.boneWeights = boneWeights;
            mesh.RecalculateBounds();

            target.sharedMaterial = skinmeshrender.sharedMaterial;
            target.sharedMesh = mesh;
            target.bones = _bones;
//                skinmeshrender.rootBone = newbones[0];// _bones[0];
            target.rootBone = _bones[0].GetChild(0);
        }
    }
    public class AnimationAdapter : MonoBehaviour
    {
        private EosPawnActor _actor;
        public void SetActor(EosPawnActor actor)
        {
            _actor = actor;
        }
        public void AniEvent(string name)
        {
            _actor.OnAnimationEvent?.Invoke(_actor, name);
        }
    }
    public class AnimationController
    {
        private Animation _animation;
        private EosPawnActor _owner;
        private string _curanimation;
        private bool _isloop;
        private int _checkerID = -1;
        public Dictionary<int, string> PlayingAnimations = new Dictionary<int, string>();
        public AnimationController(EosPawnActor owner)
        {
            _owner = owner;
        }
        public AnimationController(EosPawnActor owner, Animation animator)
        {
            _owner = owner;
            _animation = animator;
        }

        public void SetAnimation(Animation animation)
        {
            _animation = animation;
            foreach (var ani in PlayingAnimations)
            {
//                PlayNode(ani.Value);
                _animation.CrossFade(ani.Value, 0.1f, 0);
            }
        }
        public void PlayNode(string name,bool rewind = false)
        {
            _curanimation = name;
            PlayingAnimations[0] = name;
            if (_animation==null)
                return;
            _animation.CrossFade(name, 0.1f, 0);
//            var state = _animatipm.GetNextAnimatorStateInfo(0);
//            if (_checkerID != -1)
//                _owner.Ref.Coroutine.OnStopCoroutine(_checkerID);
//            var clips = _animatipm.GetCurrentAnimatorClipInfo(0);
//            if (clips.Length==0)
//            {
////                Debug.Break();
//                return;
//            }
//            _owner.Ref.Coroutine.OnCoroutineStart(CheckAniEnd(_animatipm.GetCurrentAnimatorClipInfo(0)[0].clip.length));
//            _checkerID = _owner.Ref.Coroutine.NowID;
        }
        private IEnumerator CheckAniEnd(float length)
        {
            yield return new WaitForSeconds(length);
            _owner.OnAnimationStopped?.Invoke(_owner, _curanimation);
        }
    }
    [EosObject]
    [DescendantOf(typeof(Eos.Service.Workspace))]
    public partial class EosPawnActor : EosTransformActor , IColliderEvent
    {
        public EosPawnActor()
        {
            _controller = new AnimationController(this);
        }
        [Key(330)] public Dictionary<int, string> PlayingAnimations
        {
            get => _controller.PlayingAnimations;
            set { _controller.PlayingAnimations = value; }
        }
        [IgnoreMember]public BodyOre Body { get; set; }// will be deleted..
        [RequireMold("BodyMolds")]
        [Inspector("Ore", "Body")]
        [Key(331)] public OreReference BodyOre { get; set; } = new OreReference();
        [IgnoreMember] public EosSkeleton Skeleton => _skeleton;
        private EosSkeleton _skeleton;
        private Animation _animator;
        private Rigidbody _rigidbody;
        private AnimationController _controller;
        
        [IgnoreMember] public Rigidbody Rigidbody => _rigidbody;
        [IgnoreMember] public List<EosCollider> Collders = new List<EosCollider>();
        [IgnoreMember] public EventHandler<string> OnAnimationEvent;
        [IgnoreMember] public EventHandler<string> OnAnimationStopped;
        [IgnoreMember] public EventHandler OnGearLoaded;
        [IgnoreMember] public EventHandler<Bounds> OnRefreshRenders;
            
        private EosSignal _oncollisionenter;
        private EosSignal _oncollisionexit;
        private EosSignal _ontriggerenter;
        private EosSignal _ontirggerexit;
        [IgnoreMember] public EosSignal OnCollisionEnter { get =>_oncollisionenter = _oncollisionenter??new EosSignal();}
        [IgnoreMember] public EosSignal OnCollisionExit{ get=> _oncollisionexit = _oncollisionexit??new EosSignal();}
        [IgnoreMember] public EosSignal OnTriggerEnter{ get=> _ontriggerenter = _ontriggerenter??new EosSignal();}
        [IgnoreMember] public EosSignal OnTriggerExit{ get=> _ontirggerexit = _ontirggerexit??new EosSignal();}

        #region Notify Methods
        [NotifyObjectActionMethod((int) EosObjectAction.PlayNode)]
        protected void PlayNodeAction(string name,bool rewind)
        {
            UnityEngine.Debug.Log($"Incomming animation:{name}");
            PlayNode_Implement(name,rewind);
        }
        #endregion
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosPawnActor targetpawn))
                return;
            targetpawn.Body = Body;
            targetpawn.BodyOre = BodyOre;
            base.OnCopyTo(target);
        }

        [Key(332)]
        public override bool CanCollide
        {
            get => _cancollide;
            set
            {
                _cancollide = value;
                if (_collider!=null)
                    UnityEngine.Object.Destroy(_collider);
                if (!_cancollide)
                    return;
                CreateCollider();
                AttachRigidBody();
            }
        }

        private void CreateCollider()
        {
            if (_transform.Transform == null)
                return;
            var collider = _transform.Transform.gameObject.AddComponent<CapsuleCollider>();
            eosColliderAdaptor.RegistCollisionEvent(this,null);                
            _collider = collider;
        }

        public bool RayCast()
        {
            return false;
        }

        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            if (_cancollide)
            {
                CreateCollider();
                AttachRigidBody();
            }

            OnGearLoaded += GearLoaded;
//            if (BodyOre == null)
//                return;
//            var bodyore = BodyOre.GetOre();
//            if (bodyore != null)
//                Body = bodyore.GetComponent<BodyOre>();
//            var body = Body.GetBody();
//            _skeleton = new EosSkeleton();
//            _animator = body.GetComponent<Animator>();
//            _controller = new AnimationController(this, _animator);
//            var anievent = body.AddComponent<AnimationAdapter>();
//            anievent.SetActor(this);
//            body.transform.SetParent(Transform.Transform);
//            body.transform.localPosition = Vector3.zero;
//            body.transform.localRotation = Quaternion.identity;

//            _skeleton.SetupSkeleton(Transform.Transform, body.transform);

//            var initialparts = Body.GetInitialParts();
////            var gears = FindChilds<EosGear>();
//            foreach(var part in initialparts)
//            {
//                //                if (gears.Count > 0 && gears.Exists(it => it.Part == part))
//                //                    continue;
//                var gear = ObjectFactory.CreateInstance<EosGear>();// new EosGear();
//                gear.Part = part;
//                AddChild(gear);
//            }
        }

        public override void StartPlay()
        {
            base.StartPlay();
        }

        private void GearLoaded(object sender, EventArgs args)
        {
            var gearsready = true;
            IterChilds<EosGear>((child) =>
            {
                if (!child.Ready)
                    gearsready = false;
            },true);
            if (!gearsready)
                return;
            RefreshRenderer();
            OnRefreshRenders?.Invoke(this,CalculateBounds(_transform.Transform));
        }

        protected Bounds CalculateBounds(Transform transform)
        {
            var backupscale = transform.localScale;
            var backupparent = transform.parent;

            transform.parent = null;
            transform.localScale = Vector3.one;
            var renderers = transform.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds();
            Vector3 tcenter = Vector3.zero;
            foreach (var r in renderers)
            {
                var lcenter = r.bounds.center;
                tcenter += lcenter;
            }
            tcenter /= renderers.Length;
            Bounds bound = new Bounds(tcenter,Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                bound.Encapsulate(renderers[i].bounds);
            }

            var col = _collider as CapsuleCollider;
            bound.center = bound.center - transform.position;

            transform.parent = backupparent;
            transform.localScale = backupscale;
            return bound;
        }
        private void RefreshRenderer()
        {
            // var renderers = _transform.Transform.GetComponentsInChildren<Renderer>();
            // if (renderers.Length == 0)
            //     return;
            //     return;
            // var bound = renderers[0].bounds;
            // Debug.Log($"------renderer :{renderers[0].name} center:{bound.center} bound{bound.extents}");
            // for (int i = 1; i < renderers.Length; i++)
            // {
            //     var bb = renderers[i].bounds;
            //     Debug.Log($"------renderer :{renderers[i].name} center:{bb.center} bound{bb.extents}");
            //     bound.Encapsulate(bb);
            // }

            var bound = CalculateBounds(_transform.Transform);
            var col = _collider as CapsuleCollider;
            if (col==null)
                return;
            col.center = bound.center;
            col.radius = (bound.extents.x > bound.extents.z ? bound.extents.x : bound.extents.z)*0.7f;  
            col.height = bound.extents.y * 2;
        }

        public void PlayNode(string name,bool rewind = false)
        {
            PlayNode_Implement(name,rewind);
            SendObjectAction( EosObjectAction.PlayNode,name,rewind);
        }

        private void PlayNode_Implement(string name, bool rewind = false)
        {
            PlayingAnimations[0] = name;
            _controller?.PlayNode(name,rewind);
        }

        private void AttachRigidBody()
        {
            if (Transform == null || Transform.Transform ==null)
                return;
            _rigidbody = Transform.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = Transform.AddComponent<Rigidbody>();
//                    _rigidbody.isKinematic = true;
                _rigidbody.useGravity = true;
                _rigidbody.drag = 0;
                _rigidbody.angularDrag = 0;
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                
                var zerofriction = new PhysicMaterial();
                zerofriction.dynamicFriction = zerofriction.staticFriction = 0.0f;
                zerofriction.frictionCombine = PhysicMaterialCombine.Multiply;
                _collider.material = zerofriction;
                Collders = FindChilds<EosCollider>();
            }
        }
        public override void OnChildAdded(EosObjectBase child)
        {
            if (child is EosCollider collider)
            {
                Collders.Add(collider);
                AttachRigidBody();
            }
            if (child is EosBone bone)
            {
                //bone.OnReadyEvent += (sender, arga) =>
                  {
                      _skeleton = bone.Skeleton;
                      var skel = bone.BoneRoot;
                      if (skel != null)
                      {
                          _animator = skel.GetComponent<Animation>();
                          _controller.SetAnimation(_animator);
                          var anievent = skel.gameObject.AddComponent<AnimationAdapter>();
                          anievent.SetActor(this);
                      }
                  };
            }
        }
        public override void OnChildRemoved(EosObjectBase child)
        {
            if (child is EosCollider collider)
            {
                Collders.Remove(collider);
                if (Collders.Count==0)
                {
                    GameObject.Destroy(_rigidbody);
                    _rigidbody = null;
                }
            }
        }
    }
}
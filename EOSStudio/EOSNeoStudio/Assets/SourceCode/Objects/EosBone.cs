using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MessagePack;
namespace Eos.Objects
{
    [EosObject]
    [DescendantOf(typeof(Eos.Service.Workspace))]
    [MessagePackObject]

    public partial class EosBone : EosObjectBase
    {
        [Key(21)]
        public long BoneGUID;
        private EosSkeleton _skeleton;
        private GameObject _bone;
        [IgnoreMember]public GameObject Bone;

        [IgnoreMember] public EosSkeleton Skeleton => _skeleton;
        [IgnoreMember] public Transform BoneRoot => _bone?_bone.transform:null;

        public override void OnCreate()
        {
            base.OnCreate();
            _ready = false;
        }

        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosBone bone))
                return;
            base.OnCopyTo(target);
            bone.BoneGUID = BoneGUID;
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
//            SetupBone();
        }
        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            if (!(_parent is EosTransformActor))
                return;
            OnReady();
        }
        private async Task SetupBone()
        {
            if (!(_parent is EosTransformActor parentrans))
                return;
            if (_bone != null)
                return;
            if (BoneGUID != 0)
            {
                var rsrv = Ref.Resource;
                await rsrv.IsLoadedAsset(BoneGUID);

                var bone = rsrv.GetAssetFromID<UnityEngine.Object>(BoneGUID) as GameObject;
                if (bone == null)
                {
                    try
                    {
                        await rsrv.AssetLoadTest(BoneGUID);
                        bone = rsrv.GetAssetFromID<UnityEngine.Object>(BoneGUID) as GameObject;
                        _bone = bone;
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Load asset exception:{BoneGUID}");
                        throw;
                    }
                }
                else
                {
                    _bone = GameObject.Instantiate(bone);
                    rsrv.RegistResourceRefference(BoneGUID,_bone);
                }

                //await Task.Delay(5000);

                _bone.transform.SetParent(parentrans.Transform.Transform,false);
                //var uo = Test.FastTest.Instance.GetResource(BoneGUID);
                //_bone = (uo != null) ? Object.Instantiate(uo, parentrans.Transform.Transform, false) as GameObject : new GameObject(Name);
            }
            else
            {
                _bone = GameObject.Instantiate(Bone.gameObject, parentrans.Transform.Transform, false);
            }
            _skeleton = new EosSkeleton();
            _skeleton.SetupSkeleton(parentrans.Transform.Transform, _bone.transform);           
        }
        private async Task OnReady()
        {
            await SetupBone();
            OnReadyEvent?.Invoke(this, null);
            OnReadyEvent = null;
            _ready = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            var rsrv = Ref.Resource;
            rsrv.ResourceDestroyed(BoneGUID,_bone);
        }
    }
}

using System;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace Eos.Objects
{
    public partial class EosBone : EosObjectBase
    {
        public async void test_OnActivate(bool active)
        {
            base.OnActivate(active);
            test_OnReady();
        }

        public async Task test_SetupBobyMesh(EosPawnActor parent)
        {
            if (!(parent is EosTransformActor))
                return;
            if (_bone == null)
            {
                await test_SetupBone();
                
                OnReadyEvent?.Invoke(this, null);
                OnReadyEvent = null;
                _ready = true;
                _bone.transform.SetParent(parent.Transform.Transform,false);
                // 2023-03-19 pawnactor skeleton 을 필요하게 설정해준다. 뼈는 일단 한개다..안그래?
                _skeleton = new EosSkeleton();
                _skeleton.SetupSkeleton(parent.Transform.Transform, _bone.transform);
                parent.AddChild(this);
                
                // //TestBody(parent);
                
                return;
            }

            _bone.transform.SetParent(parent.Transform.Transform,false);
        }

        // private async void TestBody(EosPawnActor parent)
        // {
        //     var _resourcesmeta = EOSResource.Instance.Resourcesmeta;
        //     var gearnames = new string[]
        //     {
        //         "Bag01",
        //         "Eye03",
        //         "Eyebrows03",
        //         "Face03",
        //         "glasses03",
        //         "Hair03",
        //         "mustache03",
        //         "Body03",
        //         "Foot03"
        //     };
        //     foreach (var gearsrc in gearnames)
        //     {
        //         var gear = ObjectFactory.CreateEosObject<EosGear>();
        //         gear.Name = gearsrc;
        //         var gearmeta = _resourcesmeta.Where(t => t.Value.NameExt == gearsrc + ".rtprefab").Select(t => t.Value);
        //         var gearmetalist = gearmeta.ToList();
        //         gear.GearGUID = gearmetalist[0].ItemID;
        //         parent.AddChild(gear);
        //         gear.test_GearOnActivate(true);
        //     }
        //
        // }
        public async Task test_SetupBone()
        {
            // 2023-3-15 (16:44) 일부러 함정을 쳐놓고 대기해본다. 나중에.(의존되면 나중에 관리하고 다음 과정으로 넘어간다)
            //                    bone 막들어짐...다음 과장으로 뻐를 구성한다.
            // if (!(_parent is EosTransformActor parentrans))
                // return;
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

                // _bone.transform.SetParent(parentrans.Transform.Transform,false);
            }
            else
            {
                // _bone = GameObject.Instantiate(Bone.gameObject, parentrans.Transform.Transform, false);
            }
            //_skeleton = new EosSkeleton();
            // _skeleton.SetupSkeleton(parentrans.Transform.Transform, _bone.transform);           
        }
        private async void test_OnReady()
        {
            await test_SetupBone();
            OnReadyEvent?.Invoke(this, null);
            OnReadyEvent = null;
            _ready = true;
        }

    }

}
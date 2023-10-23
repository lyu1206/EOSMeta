using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MessagePack;

namespace Eos.Objects
{
    [EosObject]
    public partial class EosGear : EosObjectBase
    {
        private long _gearguid;

        [Key(21)]
        public long GearGUID
        {
            get => _gearguid;
            set => _gearguid = value;
        }
        [IgnoreMember]public GameObject Part;
        public override void OnCopyTo(EosObjectBase target)
        {
            if (!(target is EosGear targetgear))
                return;
            targetgear.Part = Part;
            targetgear.GearGUID = GearGUID;
            base.OnCopyTo(target);
        }
        public override void OnCreate()
        {
            base.OnCreate();
            _ready = false;
        }
        public static void Gear(GameObject gearpart,Transform parenttrans,EosSkeleton skeleton)
        {
            var orgmesh = gearpart.GetComponentInChildren<SkinnedMeshRenderer>();
            if (orgmesh != null)
            {
                var part = ObjectFactory.CreateUnityInstance(orgmesh.name).gameObject;// GameObject.Instantiate(orgmesh.gameObject);
                var skinmeshrender = part.AddComponent<SkinnedMeshRenderer>();
                //                Debug.Log($"skinnedmesh:{Part.name}");
                part.transform.parent = parenttrans;
                //                part.name = Part.name;
                part.transform.localPosition = orgmesh.transform.localPosition;
                part.transform.localRotation = orgmesh.transform.localRotation;
                part.transform.localScale = orgmesh.transform.localScale;
                skeleton.SkinedMeshSetup(orgmesh, skinmeshrender);
            }
        }
        protected override void OnActivate(bool active)
        {
            if (!(_parent is EosPawnActor pawn))
                return;
            OnReady();
        }
        private async Task SetupGear()
        {
            if (!(_parent is EosPawnActor pawn))
                return;

            //await Task.Delay(Random.Range(1, 10)*1000);
            
            if (GearGUID != 0)
            {
                await GearLoad();
            }
            else
            {
                legacy_GearLoad();
            }
        }
        private async Task GearLoad()
        {
            var pawn = _parent as EosPawnActor;
            var rsrv = Ref.Resource;
            
            await rsrv.IsLoadedAsset(GearGUID);

            var orgpart = rsrv.GetAssetFromID<UnityEngine.Object>(GearGUID) as GameObject;
            if (orgpart == null)
            {
                await rsrv.AssetLoadTest(GearGUID);
                orgpart = rsrv.GetAssetFromID<UnityEngine.Object>(GearGUID) as GameObject;
            }
            orgpart.SetActive(false);
            var orgmesh = orgpart.GetComponentInChildren<SkinnedMeshRenderer>();
            var part = ObjectFactory.CreateUnityInstance(orgmesh.name).gameObject;// GameObject.Instantiate(orgmesh.gameObject);
            var skinmeshrender = part.AddComponent<SkinnedMeshRenderer>();
            //                Debug.Log($"skinnedmesh:{Part.name}");
            part.transform.SetParent(pawn.Transform.Transform);
            //                part.name = Part.name;
            part.transform.localPosition = orgmesh.transform.localPosition;
            part.transform.localRotation = orgmesh.transform.localRotation;
            part.transform.localScale = orgmesh.transform.localScale;
            await  pawn.Skeleton.SkinedMeshSetup(orgmesh, skinmeshrender);
            // await Task.Run(() =>
            // {
            //     await pawn.Skeleton.SkinedMeshSetup(orgmesh, skinmeshrender);
            // });
        }
        private void legacy_GearLoad()
        {
            var pawn = _parent as EosPawnActor;
            var orgmesh = Part.GetComponentInChildren<SkinnedMeshRenderer>();
            if (orgmesh != null)
            {
                var part = ObjectFactory.CreateUnityInstance(orgmesh.name).gameObject;// GameObject.Instantiate(orgmesh.gameObject);
                var skinmeshrender = part.AddComponent<SkinnedMeshRenderer>();
                //                Debug.Log($"skinnedmesh:{Part.name}");
                part.transform.SetParent(pawn.Transform.Transform);
                //                part.name = Part.name;
                part.transform.localPosition = orgmesh.transform.localPosition;
                part.transform.localRotation = orgmesh.transform.localRotation;
                part.transform.localScale = orgmesh.transform.localScale;
                pawn.Skeleton.SkinedMeshSetup(orgmesh, skinmeshrender);
            }
        }
        public override void OnAncestryChanged()
        {
        }
        private async Task OnReady()
        {
            if (!(_parent is EosPawnActor pawn))
                return;
            // block trick Skeleton 세팅하고 다시 실천해보다..담날!!!~~
            // 2023-4-7 이건 await실행의 결과를 기다리는거다 await로 멀기다힌ㄴ게 아니다..시점 기대지 맨라 속도 유지를 유지하니깐 바보는되는거다고요..으..이건 지워!! 전부다
            // await 지워지워!!
            
            await TaskExtension.WaitUntil(pawn, (p) => p.Skeleton != null);
            await SetupGear();
            _ready = true;
            pawn.OnGearLoaded?.Invoke(this,EventArgs.Empty);
            OnReadyEvent?.Invoke(this, null);
            OnReadyEvent = null;
        }

    }
}
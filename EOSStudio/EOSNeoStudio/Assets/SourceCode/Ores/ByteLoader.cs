using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eos.Objects;
using UnityEditor.Experimental;
using UnityEngine;

namespace Eos.Ores
{
    public class ByteLoader : ReferPlayer
    {
        private List<OreBase> _oredlists = new List<OreBase>();
        private Dictionary<long,uint> _ownersload = new Dictionary<long, uint>();
        public class Loaderitem
        {
            public Loaderitem(EosObjectBase obj)
            {
                _object = obj;
            }
            private EosObjectBase _object;
            
            //public void GetTarget()

        }
        private Dictionary<long,EosObjectBase> _oreloadings = new Dictionary<long, EosObjectBase>();

        private async Task _loadingOres()
        {
            // Ref.Resource.ResourceLoaded()
            // Ref.Resource.AssetLoadTest();

            await TaskExtension.WaitUntil
            (() =>
            {
                return true;
            }, 10);
        }

        public void OreLoad(long id, EosObjectBase owner)
        {
            if (_oreloadings.ContainsKey(id))
                return;
            _oreloadings.Add(id,owner);
        }
        private async void Test()
        {
            _oredlists[0].LoadOre();
            
            await TaskExtension.WaitUntil(()=>_oredlists[0].Loaded != false);
            _oredlists.RemoveAt(0);
            await TaskExtension.WaitUntil(()=>_oredlists.Count == 0);
        }

        public void test_SetOwnerGear(string gear, uint owner)
        {
            var guidlong = testDB_GearTable.GetGearGUID(gear);
            SetTools(guidlong,owner);
        }
        public IEnumerator ProcesLoad()
        {
            int count = 0; 
            yield return new WaitForSeconds(2);
            while (count < _oredlists.Count)
            {
                _oredlists[count].LoadOre();
                while (!_oredlists[count].Loaded)
                {
                    yield return 0;
                } 
                //yield return new WaitForSeconds(2);
                //yield return new WaitForEndOfFrame();
                ((GearOre)_oredlists[count]).AttachOwner(Ref.ObjectManager[(uint)count+1] );
                count++;
            }
        }
        public void SetTools(long uid, uint owner)
        {
            //Debug.Log($"Load Progress progress");
            
            // (2023-3-26) StartCoroutine에서 새로 다시 채크하고 다시 async에서 다시 다음 async를 다시 동작시킨다.다시
            // async와 StartCourtine이랑 같이 써버로 써버에서 그리고 또 먼가 gear에서 멀 아낄수 없는 데이터가 없다 요건
            // 멀 챙겨야지..으..먼가 이상하다.
            
            
            // (2023-3-27)여기 밑에다 _oredlists[_oredlists.Count-1].LoadOre(); 이거 StartCoroutine 넣어서 제대로 처리하자.ㄱㄱㄱㄱㄱ(done good)
            // (2023-3-30)역시 먼가 계속된다. Gear에서 시간차로 로딩되는것으로 StartCoroutine로 시간차를 이끌어 내니, 
            // 시간차 없이 잘 로딩 된다.그럼으로 일단 위의사항은 해결이 될것같다.일단은 StartCoroutine으로 해결된 후로 await async 를 
            // 로딩후의 해결방한으로 해결해버린다 그럼 Gear등의 Ore들의 로딩이 잘 해결될듯하다.ㄱㄱㄱ
            
            var di = new GearOre();
            di.OreId = uid;
            di.Loaded = false;
            di.GearOwnerID = owner;
            _oredlists.Add(di);
            
            //_oredlists[_oredlists.Count-1].LoadOre();
            
            if (_oredlists.Count == 1)
                Ref.StartCoroutine(ProcesLoad());
 
            
            // if (_ownersload.ContainsKey(uid))
            //     return;
            // _ownersload.Add(uid,owner);
            // if (_ownersload.Count == 1)
            //     Ref.StartCoroutine(ProcesLoad());
        }
    }
}

public static class testDB_GearTable
{
    public  static long GetGearGUID(string name)// 2023-3-26(여기서는 역시 찾아낼 Gear를 우선 만들어서 가짜로 로딩되도록 하애한다)
    {
        var _resourcesmeta = EOSResource.Instance.Resourcesmeta;
        var gearID = new string[]
        {
            "Bag01",
            "Eye03",
            "Eyebrows03",
            "Face03",
            "glasses03",
            "Hair03",
            "mustache03",
            "Body03",
            "Foot03"
        };
        var id = gearID.FindIndex(name);
        var gearmeta = _resourcesmeta.Where(t => t.Value.NameExt == name + ".rtprefab").Select(t => t.Value);
        var gearmetalist = gearmeta.ToList();
        var gearid = gearmetalist[0].ItemID;
        return gearid;
    }
}

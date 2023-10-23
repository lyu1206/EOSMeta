using System.Threading.Tasks;
using Eos.Objects;
using UnityEngine;

namespace Eos.Ores
{
    public partial class GearOre : OreBase
    {
        public override void LoadOre()
        {
            if (Loaded)
            {
                ActiveUpLoad();
            }
            GearLoad();
        }

        private void ActiveUpLoad()
        {
            
        }

        public void GearLoad(string gearname)
        {
            var gear = ObjectFactory.CreateEosObject<EosGear>();
            gear.Name = "Gear";
//            var gearid = GetGearGUID(gearname);
            //parent.AddChild(gear);
            gear.test_GearOnActivate(true);
        }

        public void AttachOwner(EosObjectBase owner)
        {
            if (!(owner is EosGear gear))
                return;
            gear.test_GearOnActivate(true);
        }

        public void UnAttackOnwer()
        {
            
        }
        private async void GearLoad()
        {
            var rsrv = Ref.Resource;
            await rsrv.IsLoadedAsset(OreId);
            
            var orgpart = rsrv.GetAssetFromID<UnityEngine.Object>(OreId) as GameObject;
            if (orgpart == null)
            {
                await rsrv.AssetLoadTest(OreId);
                orgpart = rsrv.GetAssetFromID<UnityEngine.Object>(OreId) as GameObject;
            }
            orgpart.SetActive(false);
            Loaded = true;
        }
    }
}
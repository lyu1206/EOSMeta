using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Ore
{
    using Assets;
    using MessagePack;
    using Objects;
    [MessagePack.MessagePackObject]
    public struct OreReference
    {
        [Key(331)]public int OreID;
        [Key(332)]public string Mold;
        public GameObject GetOre()
        {
            var mold = Assets.Mold.GetMold(Mold);
            return mold.GetOre<GameObject>(OreID);
        }
        public Object GetOreObject()
        {
            if (string.IsNullOrEmpty(Mold))
                return null;
            var mold = Assets.Mold.GetMold(Mold);
            if (mold == null)
                return null;
            return mold.GetOre<Object>(OreID);
        }
        public GameObject Instantiate()
        {
            if (OreID == 0)
                return null;
            var mold = Assets.Mold.GetMold(Mold);
            var src = mold.GetOre<GameObject>(OreID);
            var obj = GameObject.Instantiate(src);
            obj.name = src.name;
            return obj;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is OreReference refer))
                return false;
            return refer.OreID == OreID && refer.Mold == Mold;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
    // 모노로 사용하면 안되고,툴이 준비되면 씨리얼라이즈 객체로 교체해야한다.
    public class OreBase : MonoBehaviour
    {
        public Transform Instantiate()
        {
            return (GameObject.Instantiate(gameObject) as GameObject).transform;
        }
        public virtual Transform Instantiate(Transform parent)
        {
            var oreobj = GameObject.Instantiate(gameObject) as GameObject;
            oreobj.transform.SetParent(parent);
            oreobj.transform.localPosition = Vector3.zero;// gameObject.transform.localPosition;
            oreobj.transform.localRotation = Quaternion.identity;// gameObject.transform.localRotation;
            return oreobj.transform;
        }
    }
    public abstract class BoltLinkOre : OreBase
    {
        public abstract void SetObject(EosObjectBase eosobj);
    }

}
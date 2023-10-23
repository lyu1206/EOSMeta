using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eos.Assets
{
    using Ore;
    //using GuAsset;
    public partial class Mold : ScriptableObject
    {
        [System.Serializable]
        public class MoldItem
        {
            [SerializeField]
            private int _id;
            [SerializeField]
            private Object _item;
            public int ID => _id;
            public Object Item => _item;
        }

        public const string MoldPath = "Molds";
        [SerializeField]
        private Object[] _sources;
        [SerializeField]
        private MoldItem[] _items;
        public static Mold GetMold(string name)
        {
            var path = Path.Combine(MoldPath, name);
            var mold = Resources.Load<Mold>(path);
            return mold;
        }
        public int GetIndexInMold(OreReference ore)
        {
            return -1;// _items.FindIndex(it => it.ID == ore.OreID);
        }
        public int GetOreID(int index)
        {
            return _items[index].ID;
        }
        public T GetOre<T>(int id) where T : Object
        {
            //var item = _items.Find(it => it.ID == id);
            //if (item == null)
            //    return default(T);
            //return item.Item as T;
            return null;
        }
    }
}
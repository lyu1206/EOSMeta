using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Eos.Objects.Description
{
    using  MessagePack;
    [MessagePackObject]
    public class ObjectDescription
    {
        [Key(1)]
        public string typeName;// 나중에 string말고 매칭된 int형 아이디를 사용하자.
        [Key(2)]
        public uint objectKey;
        [Key(3)]
        public uint parentKey;

        [Key(4)] 
        public byte[] wholedata;
        [Key(5)]
        public ObjectDescription[] children;
        public static ObjectDescription GetDescription(EosObjectBase obj,bool wholedata = false)
        {
            Debug.Log($"seobj:{obj.Name} - {obj.ObjectID}");
            
            var description = new ObjectDescription {typeName = obj.GetType().Name, objectKey = obj.ObjectID,parentKey = obj.Parent!=null?obj.Parent.ObjectID:0xffffffff};
            if (wholedata)
            {
                description.typeName = null;
                var backchild = obj._children;
                obj._children = null;
                description.wholedata = MessagePackSerializer.Serialize(obj);
                obj._children = backchild;
            }
            if (obj._children.Count > 0)
            {
                var childs = new ObjectDescription[obj._children.Count];
                var index = 0;
                foreach (var child in obj._children)
                {
                    childs[index++] = GetDescription(child,wholedata);
                }
                description.children = childs;
            }
            return description;
        }

        public EosObjectBase Instantiate()
        {
            EosObjectBase eosobj = null; 
            if (wholedata != null)
            {
                eosobj = MessagePackSerializer.Deserialize<EosObjectBase>(wholedata);
                
                Debug.Log($"deobj:{eosobj.Name} - {objectKey}");
                
                var objmng = eosobj.Ref.ObjectManager;
                eosobj.ObjectID = objectKey;
                var parent = objmng[parentKey];
                objmng.RegistObject(eosobj);
                parent?.AddChild(eosobj);
            }
            if (children!=null)
                children.ForEach(c => c.Instantiate());
            return eosobj;
        }
    }
}
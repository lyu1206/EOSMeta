using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub.RTCommon;

using Battlehub.RTCommon.EditorTreeView;
using Battlehub.RTSL.Interface;
using UnityEngine.Battlehub.SL2;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.Utils;
using Battlehub.RTSL;


public class SurrogateTest : Project
{
    [SerializeField]
    private GameObject _prefab;
    private IAssetDB<long> m_assetDB;

    // Start is called before the first frame update
    protected override void Awake()
    {
        ITypeMap typeMap = new TypeMap<long>();
        IOC.Register(typeMap);
        var assetDb = new AssetDB();
        IOC.Register<IAssetDB>(assetDb);
        IOC.Register<IAssetDB<long>>(assetDb);
        base.Awake();
    }
    private void _getUnmappedObjects(GameObject go, List<UnityObject> notMapped)
    {
        if (go.GetComponent<RTSLIgnore>())
        {
            return;
        }

        if (!m_assetDB.IsMapped(go))
        {
            notMapped.Add(go);
        }

        Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; ++i)
        {
            Transform tf = transforms[i];
            if (tf.gameObject != go && !m_assetDB.IsMapped(tf.gameObject))
            {
                notMapped.Add(tf.gameObject);
            }

            Component[] components = tf.GetComponents<Component>();
            for (int j = 0; j < components.Length; ++j)
            {
                Component comp = components[j];
                if (!m_assetDB.IsMapped(comp))
                {
                    notMapped.Add(comp);
                }
            }
        }
    }
    private const string PreviewExt = ".rtview";
    private const string MetaExt = ".rtmeta";

    void Start()
    {
        IOC.Register<ISerializer>(new ProtobufSerializer());

        var assetItem = new AssetItem<long>();
        m_assetDB = IOC.Resolve<IAssetDB<long>>();
        object obj = _prefab;
        Type objType = obj.GetType();
        Type persistentType = m_typeMap.ToPersistentType(objType);
        List<UnityObject> notMapped = new List<UnityObject>();

        if (persistentType.GetGenericTypeDefinition() == typeof(PersistentGameObject<>))
        {
            persistentType = typeof(PersistentRuntimePrefab<long>);
        }

        if (obj is GameObject)
        {
            GetUnmappedObjects((GameObject)obj, notMapped);
            //            _getUnmappedObjects((GameObject)obj, notMapped);
        }
        foreach (var it in notMapped)
        {
            m_assetDB.RegisterDynamicResource((long)ToPersistentID(it), it);
        }

        var persistentObject = (PersistentRuntimePrefab<long>)Activator.CreateInstance(persistentType);
        persistentObject.ReadFrom(obj);
        var context = new GetDepsFromContext();
        persistentObject.GetDepsFrom(_prefab, context);
        var dependancies = new List<PersistentObject<long>>();
        foreach (var dep in  context.Dependencies)
        {
            var persisttype = m_typeMap.ToPersistentType(dep.GetType());
            var componentData = (PersistentObject<long>)Activator.CreateInstance(persisttype);
            componentData.ReadFrom(dep);
            dependancies.Add(componentData);
        }

        SetID(assetItem, m_assetDB.ToID((UnityObject)obj));
        assetItem.Name = ((UnityObject)obj).name;
        assetItem.Ext = GetExt(obj);
        assetItem.TypeGuid = m_typeMap.ToGuid(obj.GetType());

        IResourcePreviewUtility resourcePreview = this.gameObject.AddComponent<ResourcePreviewUtility>(); IOC.Resolve<IResourcePreviewUtility>();
        //IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
        var previewdata = resourcePreview.CreatePreviewData((GameObject)obj);
        assetItem.Preview = new Preview { PreviewData = previewdata };
        SetPersistentID(assetItem.Preview, ToPersistentID(assetItem));

        ISerializer serializer = IOC.Resolve<ISerializer>();

        var librarypath = Application.streamingAssetsPath;
        var path = librarypath;
        var previewPath = path + "/" + assetItem.NameExt + PreviewExt;

        File.Delete(previewPath);
        using (FileStream fs = File.Create(previewPath))
        {
            serializer.Serialize(assetItem.Preview, fs);
        }

        using (FileStream fs = File.Create(path + "/" + assetItem.NameExt))
        {
            serializer.Serialize(persistentObject, fs);
            assetItem.CustomDataOffset = fs.Position;
        }
        File.Delete(path + "/" + assetItem.NameExt + MetaExt);
        using (FileStream fs = File.Create(path + "/" + assetItem.NameExt + MetaExt))
        {
            serializer.Serialize(assetItem, fs);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ISerializer serializer = IOC.Resolve<ISerializer>();
            var librarypath = Application.streamingAssetsPath;
            var assetext = GetExt(typeof(GameObject));
            var path = librarypath;
            var assetPath = path + "/" + "Cube" + assetext;
            var previewPath = path + "/" + "Cube" + assetext + PreviewExt;
            PersistentRuntimePrefab<long> asset = null;
            using (FileStream fs = File.OpenRead(assetPath))
            {
                var idToObj = new Dictionary<long, UnityObject>(); 
                Transform m_dynamicPrefabsRoot = null;
                List<GameObject> createdGameObjects = new List<GameObject>();
                asset = serializer.Deserialize<PersistentRuntimePrefab<long>>(fs);
                asset.CreateGameObjectWithComponents(m_typeMap, asset.Descriptors[0], idToObj, m_dynamicPrefabsRoot, createdGameObjects);

                m_assetDB = IOC.Resolve<IAssetDB<long>>();
                m_assetDB.RegisterDynamicResources(idToObj);
                for (int j = 0; j < createdGameObjects.Count; ++j)
                {
                    GameObject createdGO = createdGameObjects[j];
//                    createdGO.hideFlags = HideFlags.HideAndDontSave;
                }
                asset.WriteTo(createdGameObjects[0]);
            }
        }
    }
}

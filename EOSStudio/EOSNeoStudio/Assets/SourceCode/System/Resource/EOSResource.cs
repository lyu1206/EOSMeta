using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Battlehub.RTSL;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using UnityEngine.Battlehub.SL2;
using Battlehub.RTSL.Battlehub.SL2;
using UnityObject = UnityEngine.Object;
using Battlehub.Utils;
using Eos.Service;
using Eos.Objects;
using Eos.Resource;
using UnityEngine;
using UnityEngine.Networking;
using MessagePack;

namespace Eos.Resource
{
    public interface IResource
    {
        Task AssetLoadTest(long resid);
        Task IsLoadedAsset(long resid);
        T GetAssetFromID<T>(long guid) where T : UnityEngine.Object;
        void ResourceDestroyed(long guid, UnityObject obj);
        void RegistResourceRefference(long guid, UnityObject obj);
        bool ResourceLoaded(long guid);
    }
}

public class EOSResource : IResource
{
    public const string MetaExt = ".rtmeta";
    private static EOSResource _instance;
    public static EOSResource Instance => _instance ?? (_instance = new EOSResource());
    private Dictionary<long, RemoteAssetItem> _resourcesmeta;
    private Dictionary<long,List<UnityObject>> _loadedResources = new Dictionary<long, List<UnityObject>>();
    private ITypeMap m_typeMap;
    private IAssetDB<long> m_assetDB;
    private HashSet<long> _loadedlist = new HashSet<long>();
    public IAssetDB<long> AssetDb => m_assetDB;
    public Dictionary<long, RemoteAssetItem> Resourcesmeta => _resourcesmeta;
    public const string RelativeAssetFolderPath = "/EOSMetaAsset";
    public string AssetFolderPath;/* => "E:/git/EOSMeta/EOSMetaAsset";*/
    public string SolutionFolderPath;/* => "E:/git/EOSMeta/EOSMetaProjects";*/
    public void Init()
    {
        IOC.Register<ITypeMap>(m_typeMap = new TypeMap<long>());
        IOC.Register<ISerializer>(new ProtobufSerializer());
        var assetDb = new AssetDB();
        IOC.Register<IAssetDB>(assetDb);
        IOC.Register<IAssetDB<long>>(assetDb);
        IUnityObjectFactory objFactory = new UnityObjectFactory();
        IOC.Register(objFactory);
        IMaterialUtil materialUtil = new StandardMaterialUtils();
        IOC.Register(materialUtil);
        IRuntimeShaderUtil shaderUtil = new RuntimeShaderUtil();
        IOC.Register(shaderUtil);

        AssetFolderPath = $"{Application.dataPath}/../..{RelativeAssetFolderPath}";
        SolutionFolderPath = Application.dataPath + "/../../EOSMetaProjects";

        Debug.Log($"Asset Data Path:{Application.dataPath}");
        
        m_assetDB = IOC.Resolve<IAssetDB<long>>();
        _resourcesmeta = GetRawResourceMeta("");
    }

    public bool ResourceLoaded(long guid)
    {
        return m_assetDB.IsMapped(guid);
    }
    private Dictionary<long, RemoteAssetItem> GetRawResourceMeta(string folder)
    {
        var result = new Dictionary<long, RemoteAssetItem>();
        var serializer = IOC.Resolve<ISerializer>();
        var rawresourcepath = AssetFolderPath;//Application.streamingAssetsPath + folder;
        var storage = IOC.Resolve<IStorage<long>>();
        string[] files = Directory.GetFiles(rawresourcepath, "*" + MetaExt, SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i];
            // Debug.Log($"File:{file}");
            //if (!File.Exists(file.Replace(MetaExt, string.Empty)))
            //{
            //    continue;
            //}

            var assetItem = LoadItem<RemoteAssetItem>(serializer, file);
            // Debug.Log($"{assetItem.ItemID} - {assetItem.NameExt}");
            result.Add(assetItem.ItemID, assetItem);
        }
        return result;
    }
    private static T LoadItem<T>(ISerializer serializer, string path) where T : ProjectItem, new()
    {
        T item = Load<T>(serializer, path);

        string fileNameWithoutMetaExt = Path.GetFileNameWithoutExtension(path);
        item.Name = Path.GetFileNameWithoutExtension(fileNameWithoutMetaExt);
        item.Ext = Path.GetExtension(fileNameWithoutMetaExt);

        return item;
    }
    public static T Load<T>(ISerializer serializer, string path) where T : new()
    {
        string metaFile = path;
        T item;
        if (File.Exists(metaFile))
        {
            try
            {
                using (FileStream fs = File.OpenRead(metaFile))
                {
                    item = serializer.Deserialize<T>(fs);
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Unable to read meta file: {0} -> got exception: {1} ", metaFile, e.ToString());
                item = new T();
            }
        }
        else
        {
            item = new T();
        }

        return item;
    }

    public void GetDependancies(long id, Stack<long> dep)
    {
        var meta = _resourcesmeta[id];
        dep.Push(id);
        if (meta.Dependencies == null)
            return;
        foreach (var it in meta.Dependencies)
            GetDependancies(it, dep);
    }

    public void SaveSolution(Solution solution)
    {
        // var typemap = IOC.Resolve<ITypeMap>();
        // Type objType = solution.GetType();
        // Type persistentType = m_typeMap.ToPersistentType(typeof(RuntimeSolution));
        // var rtsolution = ScriptableObject.CreateInstance<RuntimeSolution>();
        // rtsolution.Solution = solution;
        // var serializer = IOC.Resolve<ISerializer>();
        // var persistentObject = Activator.CreateInstance(persistentType) as PersistentObject<long>;
        // persistentObject.ReadFrom(rtsolution);
        //
        // #region Save Workspace to Solution
        // var solutionpath = SolutionFolderPath + "/";
        // using (FileStream fs = File.Create(solutionpath + "FastTest.solution"))
        // {
        //     serializer.Serialize(persistentObject, fs);
        // }
        // #endregion
        
        var solutionpath = SolutionFolderPath + "/";
        using (FileStream fs = File.Open(solutionpath + "FastTest.solution", FileMode.Create))
        {
            MessagePack.MessagePackSerializer.Serialize(fs,solution);
        }
    }

    public T GetAssetFromID<T>(long guid) where T : UnityEngine.Object
    {
        if (_loadedResources.ContainsKey(guid) && _loadedResources[guid].Count>0)
        {
            return _loadedResources[guid][0] as T;
        }
        var assetdb = IOC.Resolve<IAssetDB<long>>();
        var obj = assetdb.FromID<UnityEngine.Object>(guid) as GameObject;
        if (obj != null)
        {
            if (!_loadedResources.ContainsKey(guid))
                _loadedResources.Add(guid,new List<UnityObject>());
            _loadedResources[guid].Add(obj);
        }
        return obj as T;
    }

    public void RegistResourceRefference(long guid, UnityObject obj)
    {
        if (!_loadedResources.ContainsKey(guid))
            return;
        _loadedResources[guid].Add(obj);
    }
    public void ResourceDestroyed(long guid,UnityObject obj)
    {
        if (!_loadedResources.ContainsKey(guid))
            return;
        _loadedResources[guid].Remove(obj);
        if (_loadedResources[guid].Count == 0)
        {
            var assetdb = IOC.Resolve<IAssetDB<long>>();
            assetdb.UnregisterDynamicResource(guid);
        }
    }

    public async Task IsLoadedAsset(long resid)
    {
        await TaskExtension.WaitUntil(()=>!_loadedlist.Contains(resid));
    }
    public async Task AssetLoadTest(long resid)
    {
        if (_loadedlist.Contains(resid))
            return;
        _loadedlist.Add(resid);
        var deps = new Stack<long>();
        GetDependancies(resid, deps);

        ISerializer serializer = null;
        serializer = IOC.Resolve<ISerializer>();
        // await Task.Run(() =>
        foreach (var assetid in deps)
        {
            var rr = _resourcesmeta[assetid];
            var req = UnityWebRequest.Get(rr.Path + rr.NameExt);
            
            await req.SendWebRequest();
            
            // 2023-04-23(여기는 잘된다..단지 myAssetTest에서 Session 이상하고. Bone 어쩌고 저쩌고 한 myASyncTest.cs 주석단거 처리하자!!!
            
            
            //Debug.Log($"try to load path:{rr.Path} - {rr.NameExt}");
            //await Task.Delay(500);
            if (req.isDone)
            {
                var data = req.downloadHandler.data;
                var type = m_typeMap.ToType(rr.TypeGuid);
                int yyyy = 0;
                PersistentObject<long> item = null;
                // await Task.Run(() =>
                // {
                    item = await Task.Run(()=>serializer.Deserialize<PersistentObject<long>>(data));

                    if (type == typeof(GameObject))
                    {
                        var idtoobj = new Dictionary<long, UnityObject>();
                        var prefab = item as PersistentRuntimePrefab<long>;
                        List<GameObject> createdGameObjects = new List<GameObject>();
                        prefab.CreateGameObjectWithComponents(m_typeMap, prefab.Descriptors[0], idtoobj, null,
                            createdGameObjects);
                        foreach (var idobj in idtoobj)
                        {
                            m_assetDB.RegisterDynamicResource(idobj.Key, idobj.Value);
                        }
                        
                        prefab.WriteTo(createdGameObjects[0]);
                        m_assetDB.RegisterSceneObject(assetid, createdGameObjects[0]);
                    }
                    else
                    { 
                        var factory = IOC.Resolve<IUnityObjectFactory>();
                        var unitydpobjtype = m_typeMap.ToUnityType(item.GetType());
                        if (unitydpobjtype != null)
                        {
                           if (factory.CanCreateInstance(unitydpobjtype, item))
                           {
                                UnityObject assetInstance = factory.CreateInstance(unitydpobjtype, item);
                                if (assetInstance != null)
                                {
                                    m_assetDB.RegisterSceneObject(assetid, assetInstance);
                                    item.WriteTo(assetInstance);
                                    //item.WriteTo(assetInstance);
                                }
                           }
                        }
                    }
                 // });
            }
        }

        _loadedlist.Remove(resid);
    }
    public string ReadScript(string name)
    {
        var path = $"{Application.streamingAssetsPath}/Scripts/{name}";
        return File.ReadAllText(path);
    }

    public void ShutDown()
    {
        m_assetDB.UnregisterDynamicResources();
        m_assetDB.UnregisterSceneObjects();
    }
    public Solution OpenSolution(string path)
    {
        var msgpackData = File.ReadAllBytes(path);
        EosPlayer.EosPlayer.Instance.ObjectManager.SetRegistMode(ObjectRegistMode.CreateKey);
        EosPlayer.EosPlayer.Instance.ObjectManager.SetRegistType(ObjectType.Loaded);
        var desolution = MessagePackSerializer.Deserialize<Solution>(msgpackData, MessagePackSerializerOptions.Standard);
        return desolution as Solution;
        
        //_solution = desolution.CreateForEditor(null);

        //if (_solution != null)
        //    GameObject.DestroyImmediate(_solution.gameObject);
        //_objectlist.Clear();
        //_solution = CreateObject(desolution);
        //desolution.IterChildRecursive((parent, obj) =>
        //{
        //    var mono_parenteosobject = _objectlist.Find(o => o.Object == parent);
        //    CreateObject(obj, mono_parenteosobject);
        //    return true;
        //});
        //_objectlist.ForEach(o => o.Object.Created());

        //// _objectlist.ForEach(o=>o.Object.ID = o.Object.GetHashCode());

        //// _objectlist.ForEach(o => o.Object.EditorCreate(o));
    }
    public Solution OpenSolutionTest()
    {
        // var solutionpath = EOSResource.Instance.SolutionFolderPath + "/";
        // var serializer = IOC.Resolve<ISerializer>();
        // using (FileStream fs = File.Open(solutionpath + "FastTest.solution", FileMode.Open))
        // {
        //     var solution = serializer.Deserialize<PersistentRuntimeSolution<long>>(fs);
        //     var rtsolution = ScriptableObject.CreateInstance<RuntimeSolution>();
        //     solution.WriteTo(null);
        //     return solution.Solution;
        // }
        
        var solutionpath = SolutionFolderPath + "/";
        return OpenSolution(solutionpath + "FastTest.solution");
    }
    public void LoadAndSolutionTest()
    {
        var rts = OpenSolutionTest();

        // var cam = rts.FindDeepChild<EosCamera>();
        // var avt = rts.FindDeepChild<EosPawnActor>();
        // cam.LocalPosition = new Vector3(0, 10, 10);
        // cam.LookAt(avt);


        EosPlayer.EosPlayer.Instance.SetSolution(rts as Solution);
        EosPlayer.EosPlayer.Instance.Play();
    }

}

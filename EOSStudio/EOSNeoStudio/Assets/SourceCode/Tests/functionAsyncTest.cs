using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Battlehub.RTSL;
using Battlehub.RTCommon;
//using Battlehub.RTNavigation;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Eos;
using Eos.Objects;
using Eos.Ores;
using GuCore;
using ICSharpCode.SharpZipLib.Tar;
//using Ludiq;
using UnityEditor.VersionControl;
using UnityEngine.Battlehub.SL2;
using UnityEngine.Networking;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;
using UnityObject = UnityEngine.Object;


public class functionAsyncTest : Project
{
    public override void Awake_Internal()
    {
        Init();
        base.Awake_Internal();
    }
    private void Init()
    {
        EOSResource.Instance.Init();
        EosPlayer.EosPlayer.Instance.StartGame();
    }

    private void PawnActorTest()
    {
        var pawn = new EosPawnActor();pawn.Name = "Actor";
        
        pawn.OnCreate();
        pawn.Transform.Transform.transform.localPosition = new Vector3(0,0,42);
        pawn.Transform.Transform.transform.localRotation = Quaternion.Euler(0,180,0);
        pawn.Activate(true);
    }

    private async UniTask<PersistentObject<long>> LoadWebItem(long resid)
    {
        var rr = EOSResource.Instance.Resourcesmeta[resid];
        var req = UnityWebRequest.Get(rr.Path + rr.NameExt);
        await req.SendWebRequest();
        if (req.isDone)
        {
            var data = req.downloadHandler.data;
            return await UniTask.RunOnThreadPool(() =>
            {
                ISerializer serializer = IOC.Resolve<ISerializer>();
                return serializer.Deserialize<PersistentObject<long>>(data);
            });
        }
        else
        {
            throw new Exception("not loaded Asset Item.");
        }
    }

    private async UniTask GameObjectAsset(long assetid,PersistentObject<long> item)
    {
        var eff = EOSResource.Instance;
        var idtoobj = new Dictionary<long, UnityObject>();
        var prefab = item as PersistentRuntimePrefab<long>;
        List<GameObject> createdGameObjects = new List<GameObject>();
        prefab.CreateGameObjectWithComponents(m_typeMap, prefab.Descriptors[0], idtoobj, null,
            createdGameObjects);
        foreach (var idobj in idtoobj)
        {
            eff.AssetDb.RegisterDynamicResource(idobj.Key, idobj.Value);
        }
                        
        prefab.WriteTo(createdGameObjects[0]);
        eff.AssetDb.RegisterSceneObject(assetid, createdGameObjects[0]);
    }

    private async UniTask UnityObjectAsset(long  assetid,PersistentObject<long> item)
    {
        var eff = EOSResource.Instance;
        var factory = IOC.Resolve<IUnityObjectFactory>();
        var unitydpobjtype = m_typeMap.ToUnityType(item.GetType());
        if (unitydpobjtype != null)
        {
            if (factory.CanCreateInstance(unitydpobjtype, item))
            {
                UnityObject assetInstance = factory.CreateInstance(unitydpobjtype, item);
                if (assetInstance != null)
                {
                    eff.AssetDb.RegisterSceneObject(assetid, assetInstance);
                    //var eee = new UniTaskVoid();
                    item.WriteTo(assetInstance);
                    // await UniTask.fiSwitchToMainThread();
                    // await UniTask.WaitForFixedUpdate();
                    //item.WriteTo(assetInstance);
                }
            }
        }
    }
    IEnumerator loadWeb(long id)
    {
        var rr = EOSResource.Instance.Resourcesmeta[id];
        var req = UnityWebRequest.Get(rr.Path + rr.NameExt);
        yield return req.SendWebRequest();
        // if (req.isDone)
        // {
        //     var data = req.downloadHandler.data;
        //     ISerializer serializer = IOC.Resolve<ISerializer>();
        //     yield return serializer.Deserialize<PersistentObject<long>>(data);
        //     //yield return UniTask.RunOnThreadPool(() => serializer.Deserialize<PersistentObject<long>>(data));
        // }
        Debug.Log($"rcv webdata:{ req.downloadHandler.data.Length}");
        yield return 0;
    }

    IEnumerator loadCoRoutine()
    {
        PawnActorTest();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        yield return loadWeb(8589955348);
        yield return loadWeb(8589955346);
        yield return loadWeb(8589955336);
        yield return loadWeb(8589955344);
        yield return loadWeb(8589955342);
        yield return loadWeb(8589955340);
        yield return loadWeb(8589955338);
        yield return loadWeb(8589955250);
    }

    private async UniTask LoadTest()
    {
        codeview = true;
  
        
        // await UniTask.Delay(9000);
        // Debug.Log($"Enter LoadTest....");
        
        // StartCoroutine(loadCoRoutine());
        // return;

        //2023-4-30 photon unity Server에서 먼가를 기색을 부린다.전번에 이야기했던
        //이상한 속도 증상이 이상하다..이걸 일단 없애고 속도를 잡아야하고 이때 uniTask.Delay(1000)
        //에서 먼가 속도를 나온다.오늘 바루 보자!!
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        PawnActorTest();
        // await UniTask.Yield(PlayerLoopTiming.LastUpdate);
        // await UniTask.Yield(PlayerLoopTiming.LastUpdate);
        // await UniTask.Yield(PlayerLoopTiming.LastUpdate);
        // await UniTask.ToCoroutine(UniTask.Defer(loadCoRoutine));
        
        
        PersistentObject<long> item;
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955348);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955348, item);
        Debug.Log($"UnityObjectAsset :{8589955348}");

        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955346);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955346, item);
        Debug.Log($"UnityObjectAsset :{8589955346}");

        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955336);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955336, item);
        Debug.Log($"UnityObjectAsset :{8589955336}");
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955344);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955344, item);
        Debug.Log($"UnityObjectAsset :{8589955344}");
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955342);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955342, item);
        Debug.Log($"UnityObjectAsset :{8589955342}");
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955340);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955340, item);
        Debug.Log($"UnityObjectAsset :{8589955340}");
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955338);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await UnityObjectAsset(8589955338, item);
        Debug.Log($"UnityObjectAsset :{8589955338}");
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        item = await LoadWebItem(8589955250);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        await GameObjectAsset(8589955250, item);
        Debug.Log($"GameObjectAsset :{8589955250}");

        
        
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

        // //eobone.test_OnActivate(true);  //recursive active to child
        // // 2023-3-17 본의 하이어라키 이후에 구성된다.메쉬 로딩후 본의 위치에 메쉬를 위치시킨다. 
        // // 그리고,PawnActor 위치에 본(Bone)의 위치를 가질수 있도록 위치의 하위위치로 적용시킨다.
        // // 부칠수가 있도록 추가해서 똑바루 해라!!
        // var eobone = new EosBone();
        // eobone.BoneGUID = 8589955250;
        // eobone.ObjectID = 1234;

//        await eobone.test_SetupBobyMesh(pawn);
//        await eobone.test_SetupBone();
//        await EOSResource.Instance.AssetLoadTest(8589955250);
 
        
        //pawn.AddChild(eobone);
        return;

//        loadanyway(eobone.BoneGUID);

        // eobone.Ref.ByteLoader.test_SetOwnerGear("Bag01",1234);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("Eye03",1235);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("Eyebrows03",1236);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("Face03",1237);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("glasses03",1238);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("Hair03",1239);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("mustache03",1240);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("Body03",1241);
        // eobone.Ref.ByteLoader.test_SetOwnerGear("Foot03",1242);
        //
        // var gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Bag01");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Eye03");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Eyebrows03");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Face03");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("glasses03");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Hair03");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("mustache03");        await eobone.test_SetupBobyMesh(pawn);
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Body03");
        // pawn.AddChild(gear);
        //
        // gear = ObjectFactory.CreateEosObject<EosGear>();gear.Ref.ObjectManager.RegistObject(gear);
        // gear.GearGUID = testDB_GearTable.GetGearGUID("Foot03");
        // pawn.AddChild(gear);


        /*
        var gear = new EosGear();
        pawn.AddChild(gear);
        gear.Name = "Gerar#1";

        gear.GearGUID = 8589955252;
        
        gear.OnCreate();
        gear.Activate(true);
        */
    }
    private async void TestBody(EosPawnActor parent)
    {
        var _resourcesmeta = EOSResource.Instance.Resourcesmeta;
        var gearnames = new string[]
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
        foreach (var gearsrc in gearnames)
        {
            var gear = ObjectFactory.CreateEosObject<EosGear>();
            gear.Name = gearsrc;
            var gearmeta = _resourcesmeta.Where(t => t.Value.NameExt == gearsrc + ".rtprefab").Select(t => t.Value);
            var gearmetalist = gearmeta.ToList();
            gear.GearGUID = gearmetalist[0].ItemID;
            parent.AddChild(gear);
            gear.test_GearOnActivate(true);
        }

    }

    // Start is called before the first frame update
    private ByteLoader _loader = new ByteLoader();
    async Task Start()
    {
        await Task.Delay(9000);
        await UniTask.Yield(PlayerLoopTiming.LastUpdate);
        await LoadTest();
        //EosPlayer.EosPlayer.Instance.Coroutine.OnCoroutineStart(_loader.ProcesLoad());
    }
    [Range(10, 150)]
    public int fontSize = 30;
    public Color color = new Color(.0f, .0f, .0f, 1.0f);
    public float width, height;
    private float ms;
    private float fpsDisplayer;
    private float leftfpsDisplayer = 500;
    private bool codeview = false;
    void OnGUI()
    {
        var current = Event.current;
        if (current.type == EventType.KeyDown && current.keyCode == KeyCode.P)
        {
            codeview = true;
        }
        Rect position = new Rect(width, height, Screen.width, Screen.height);

        float fps = 1.0f / Time.deltaTime;
        ms += Time.deltaTime;
        if (ms > 0.2f)
        {
            fpsDisplayer = fps;
            if (codeview && leftfpsDisplayer > fpsDisplayer)
            {
                leftfpsDisplayer = fpsDisplayer;
            }
            ms = 0;
        }

        GUIStyle style = new GUIStyle();

        style.fontSize = fontSize;
        style.normal.textColor = color;

        GUI.Label(position, $"FPS : {fpsDisplayer}", style);
        position.y += 30;
        GUI.Label(position, $"LEFTFPS : {leftfpsDisplayer}", style);
    }
}

public class UnityTestAttribute : Attribute
{
}
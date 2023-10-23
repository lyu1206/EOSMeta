using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Battlehub.RTSL;
using Battlehub.RTCommon;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Eos;
using Eos.Objects;
using Eos.Ores;
using ICSharpCode.SharpZipLib.Tar;
using UnityEngine.Battlehub.SL2;
using UnityEngine.Networking;

public class myASyncTest : Project
{
    public override void Awake_Internal()
    {
        Init();
        base.Awake_Internal();
    }
    private void Init()
    {
        EOSResource.Instance.Init();
        //EosPlayer.EosPlayer.Instance.StartGame();
    }

    async void LoadTest()
    {
        // var pawn = new EosPawnActor();pawn.Name = "Actor";
        //
        // pawn.OnCreate();
        // pawn.Transform.Transform.transform.localPosition = new Vector3(0,0,42);
        // pawn.Transform.Transform.transform.localRotation = Quaternion.Euler(0,180,0);
        // pawn.Activate(true);
        //
        //
        // //eobone.test_OnActivate(true);  //recursive active to child
        // // 2023-3-17 본의 하이어라키 이후에 구성된다.메쉬 로딩후 본의 위치에 메쉬를 위치시킨다. 
        // // 그리고,PawnActor 위치에 본(Bone)의 위치를 가질수 있도록 위치의 하위위치로 적용시킨다.
        // // 부칠수가 있도록 추가해서 똑바루 해라!!
        // var eobone = new EosBone();
        // eobone.BoneGUID = 8589955250;
        // eobone.ObjectID = 1234;

//        await eobone.test_SetupBobyMesh(pawn);
//        await eobone.test_SetupBone();
        await EOSResource.Instance.AssetLoadTest(8589955250);
 
        
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
//        yield return new WaitForSeconds(8);
        LoadTest();
        //EosPlayer.EosPlayer.Instance.Coroutine.OnCoroutineStart(_loader.ProcesLoad());
    }

    // Update is called once per frame
    void Update()
    {
         // if (Input.GetKeyDown(KeyCode.T))
         //     LoadTest();
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Management.Instrumentation;
using Eos.Network;
using Eos.Objects.Description;
//using Eos.Test;
using ExitGames.Client.Photon;
using MessagePack;
using MessagePack.Resolvers;
//using Microsoft.SqlServer.Server;
using Photon.Pun;
using UnityEngine;

namespace Eos.Service
{
    using Objects;
    [NoCreated]
    public class Player : EosObjectBase 
    #if UNITY_EDITOR
    , ITransform
    #endif
    {
        private EosHumanoid _humanoid;
        private EosObjectBase _playeroot;
        
        [IgnoreMember]public EosHumanoid Humanoid => _humanoid;
        [Key(21)]
        public int PlayerID;

        [EosKeyTag(22,(int)EosPropertyNotify.HumanoidID )] public uint _humanoidid { get; set; }
        [Key(23)] public uint _playerrootid { get; set; }
        [IgnoreMember]public bool IsLocalPlayer;

        [MessagePack.IgnoreMember]
        public override string Name 
        { 
            get => base.Name; 
            set
            {
                base.Name = value;
                _transform.Name = value;
            }
        }
        private EosTransform _transform;
        
        [IgnoreMember]public EosTransform Transform => _transform;
        public Player()
        {
            _transform = ObjectFactory.CreateInstance<EosTransform>();
            _transform.Create(Name);
        }
        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            if (!(_parent is ITransform transactor))
                return;
            _transform.SetParent(transactor.Transform);
        }

        public void SetupModel()
        {
            var om = Ref.ObjectManager;
            om.SetRegistType(ObjectType.Sync);
            
            var ws = Ref.Solution.Workspace;
            var objroot = ObjectFactory.CreateEosObject<EosTransformActor>();objroot.Name = "PlayerRoot";
            _playeroot = objroot;
            _playeroot.NotifyOut = IsLocalPlayer;
            _playerrootid = objroot.ObjectID;
            ws.AddChild(objroot);
            
            var humanoid = ObjectFactory.CreateEosObject<EosHumanoid>();humanoid.Name = "Humanoid";
            
            
            objroot.AddChild(humanoid);
            
            var avatar = ObjectFactory.CreateEosObject<EosPawnActor>();avatar.Name = EosHumanoid.humanoidroot;
            var bone = ObjectFactory.CreateEosObject<EosBone>();bone.Name = "bone";
            bone.BoneGUID = 8589955250;//8589954340;
            avatar.AddChild(bone);
            
            objroot.AddChild(avatar);
            avatar.LocalPosition = new Vector3(0, 13, 0);
            avatar.LocalScale = Vector3.one;
            
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
                avatar.AddChild(gear);
            }

            var testscript = ObjectFactory.CreateEosObject<EosScript>();
            testscript.Name = "Script";
            testscript.LuaScript = EOSResource.Instance.ReadScript("player.lua");
            humanoid.AddChild(testscript);
            
            if (IsLocalPlayer)
            {
                var maincamera = EosCamera.Main;
                maincamera.Target = avatar;
                maincamera.LocalPosition = new Vector3(0, 10 * 10, 10 * 10);
                maincamera.LookAt(avatar);
            }
            
            objroot.Activate(true,true);
            objroot.StartPlay();

            _humanoid = humanoid;
            _humanoidid = humanoid.ObjectID;
            
            om.SetRegistType(ObjectType.NonSync);
        }

        IEnumerator fff(float w)
        {
            yield return new WaitForSeconds(w);
            base.OnActivate(true);
            if (Parent is Players players)
                players.PlayerConnected(this);
            if (Ref.SessionManager.IsServer)
            {
                SetupModel();
            }
            // else
            {
                IsLocalPlayer = PhotonNetwork.LocalPlayer.ActorNumber == PlayerID;
                _humanoid = Ref.ObjectManager[_humanoidid] as EosHumanoid;
                _playeroot = Ref.ObjectManager[_playerrootid] as EosObjectBase;
                _playeroot.NotifyOut = IsLocalPlayer;
                if (IsLocalPlayer)
                {
                    Ref.Solution.Players.LocalPlayer = this;
                }
            }
        }
        protected override void OnActivate(bool active)
        {
         //   FastTest.Instace.StartCoroutine(fff(6));
            base.OnActivate(active);
            if (Parent is Players players)
                players.PlayerConnected(this);
            if (Ref.SessionManager.IsServer)
            {
                SetupModel();
            }
            // else
            {
                IsLocalPlayer = PhotonNetwork.LocalPlayer.ActorNumber == PlayerID;
                _humanoid = Ref.ObjectManager[_humanoidid] as EosHumanoid;
                _playeroot = Ref.ObjectManager[_playerrootid] as EosObjectBase;
                _playeroot.NotifyOut = IsLocalPlayer;
                if (IsLocalPlayer)
                {
                    Ref.Solution.Players.LocalPlayer = this;
                }
            }
        }
        protected override void OnStartPlay()
        {
            // var model = FindChild<EosModel>();
            // if (model == null)
            //     return;
            // if (Ref.Solution.Terrain == null)
            //     return;
            // var spainplayer = Ref.Solution.Terrain.FindNode("obj_SpawnIn");
            // var humanoid = _humanoid = model.FindChild<EosHumanoid>();
            // humanoid.SetPosition(spainplayer.position);


            if (!Ref.SessionManager.IsServer && IsLocalPlayer)
            {
                var maincamera = EosCamera.Main;
                maincamera.Target = _humanoid.Humanoidroot;
                maincamera.LocalPosition = new Vector3(0, 10, 10);
                maincamera.LookAt(_humanoid.Humanoidroot);
            }
            // humanoidroot.Transform.position = spainplayer.position;
            // humanoidroot.Transform.rotation = spainplayer.rotation;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _transform.Destroy();
            _playeroot?.Destroy();
        }
    }
    [NoCreated]
    public class Players : EosService
    {
        private Player _localplayer;

        public Player LocalPlayer
        {
            get=> _localplayer;
            set => _localplayer = value;
        }
        private Dictionary<int,Player> _players = new Dictionary<int, Player>();
        public Players()
        {
            Name = "Players";
            Ref.SessionManager.PlayerConnected += OnPlayerConnected;
            Ref.SessionManager.PlayerDisConnected += OnPlayerDisConnected;
            Ref.SessionManager.JoinedRoom += OnCreatedOrJoinRoom;
        }

        public void PlayerConnected(Player player)
        {
            if (_players.ContainsKey(player.PlayerID))
                return;
            _players[player.PlayerID] = player;
        }
            
        private void OnCreatedOrJoinRoom(int localactornum)
        {
            var room = PhotonNetwork.CurrentRoom;
            var solution = EOSResource.Instance.OpenSolutionTest();
            Ref.SetSolution(solution as Solution);
            Ref.StartGame();
            // request player to masterclient(Server)
        }
        private Player CreatePlayer(int actornum)
        {
            var player = ObjectFactory.CreateEosObject<Player>();
            player.Name = $"player{actornum}";
            player.PlayerID = actornum;
            return player;
        }
        private void OnPlayerConnected(int actornum, bool islocalplayer)
        {
            // int actornum = photonplayer.ActorNumber;
            // var testhash = new Hashtable();
            // testhash["r"] = 33; // 이걸 가지고,마스터 클라이언트 정보 이관에 쓴다.ㅇㅋ?
            // photonplayer.SetCustomProperties(testhash);
                
            if (Ref.SessionManager.IsServer)
            {
                var player = CreatePlayer(actornum);
                player.IsLocalPlayer = islocalplayer;
                AddChild(player);
                player.Activate(true);
                player.StartPlay();
            }
            else if (islocalplayer)
            {
                    RequestPlayertPacket packet = new RequestPlayertPacket();
                    packet.playerID = actornum;
                    Ref.SessionManager.SendPacket((int) C2S.Commands.RequestPlayer,MessagePackSerializer.Serialize(packet));
            }
            // if (islocalplayer)
            // {
            //     var player = OnConnectPlayer(1);
            //     _players[actornum] = player;
            //     var humanoid = _localplayer.Humanoid;
            //     var maincamera = EosCamera.Main;
            //     maincamera.Target = humanoid.Humanoidroot;
            // }
            // else
            // {
            //     var otherplayer = CreatePlayer(actornum);
            //     otherplayer.Name = $"player{actornum}";
            //     AddChild(otherplayer);
            //     otherplayer.Activate(true);
            //     _players[actornum] = otherplayer;
            //     if (PhotonNetwork.IsMasterClient)
            //     {
            //         BroadcastPlayers(otherplayer);
            //     }
            // }
        }

        private void OnPlayerDisConnected(int actornum)
        {
            if (!_players.ContainsKey(actornum))
                return;
            var player = _players[actornum];
            _players.Remove(actornum);
            player.Destroy();
        }
        private void SendPlayers(Player targetplayer)
        {
            var om = Ref.ObjectManager;   
            om.Log();
            
            var descriptions = new List<ObjectDescription>();
            foreach (var playerpair in _players)
            {
                var player = playerpair.Value;
                descriptions.Add(ObjectDescription.GetDescription(player,true));
            }

            var ws = Ref.Solution.Workspace;
             
            Debug.Log($"SendPlayers ----- : Workspace ({ws.ObjectID})");
            
            ws.IterChilds((child) =>
            {
                if (ObjectFactory.GetRegistType(child)== ObjectType.Loaded)
                    return;
                descriptions.Add(ObjectDescription.GetDescription(child,true));
            });
            var createobjectpacket = new CreateObjectPacket();
            createobjectpacket.description = descriptions;
            
            Debug.Log($"&&&&&&&&&&&&&&&&&&&&&&&&&&send object infos count:{descriptions.Count}");
            
            Ref.SessionManager.SendPacket(targetplayer.PlayerID, (int)S2C.Commands.CreatePlayers,MessagePack.MessagePackSerializer.Serialize(createobjectpacket));
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        [Commands((int) C2S.Commands.RequestPlayer)]
        private void OnRequestPlayer(CmdPacket packet)
        {
            var reqplayer = MessagePack.MessagePackSerializer.Deserialize<RequestPlayertPacket>(packet.data);
            if (!_players.ContainsKey(reqplayer.playerID))
                return;
            SendPlayers(_players[reqplayer.playerID]);
        }

        [Commands((int) S2C.Commands.CreatePlayers)]
        private void OnCreatePlayers(CmdPacket packet)
        {
            var om = Ref.ObjectManager; 
            om.SetRegistMode(ObjectRegistMode.NoCreateKey);
            var createplayers = MessagePack.MessagePackSerializer.Deserialize<CreateObjectPacket>(packet.data,MessagePack.Resolvers.ContractlessStandardResolver.Options);
            var description = createplayers.description;
            var activelist = new List<EosObjectBase>();
            var root = Ref.Solution;
            
            om.Log();
            
            Debug.Log($"&&&&&&&&&&&&&&&&&&&&&&&&&&recv object infos count:{description.Count}");
                
            description.ForEach(d =>
            {
                var eosobj = d.Instantiate(); 
                if (eosobj.Parent!=null && eosobj.Parent.Parent==root)
                    activelist.Add(eosobj);
            });
            activelist.ForEach(child => child.Activate(child.Active));
            activelist.ForEach(child => child.StartPlay());
            Ref.ObjectManager.SetRegistMode(ObjectRegistMode.CreateKey);
        }
    }
}
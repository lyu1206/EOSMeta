using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using ExitGames.Client.Photon;
using Photon.Pun;
using  Eos.Resource;
//using Photon.Realtime;
using System.Threading.Tasks;
using Eos.Network;
using Eos.Ores;
using ExitGames.Client.Photon;
using MessagePack;
using MessagePack.Resolvers;

namespace Eos.Objects
{
    public enum ObjectRegistMode : uint
    {
        CreateKey = 0,
        NoCreateKey = 1,
    }
    public class ObjectManager : ReferPlayer
    {
        private Dictionary<uint,EosObjectBase> _objectlist = new Dictionary<uint, EosObjectBase>();
        private Dictionary<int,EosObjectBase> _unitylist = new Dictionary<int, EosObjectBase>();
        private HashSet<uint> _notifyobjects = new HashSet<uint>();
        private List<EosObjectBase> _updateobjectlist = new List<EosObjectBase>();
        private List<EosObjectBase> _latedeleteupdateobjectlist = new List<EosObjectBase>();
        private List<EosObjectBase> _lateaddupdateobjectlist = new List<EosObjectBase>();
        private List<Tuple<EosObjectAction,EosNotifyObject>> _objectactionlist  = new List<Tuple<EosObjectAction, EosNotifyObject>>();
        private ObjectType _registmodeType = ObjectType.NonSync;
        private ObjectRegistMode _registMode = ObjectRegistMode.CreateKey;
        public EventHandler<EosObjectBase> OnUnRegistObject;
        public EosObjectBase this[uint index]
        {
            get
            {
                if (_objectlist.ContainsKey(index))
                    return _objectlist[index];
                return null;
            }
        }
        static private uint objectkey = 1;

        public void SetRegistType(ObjectType type)
        {
            _registmodeType = type;
        }
        public void SetRegistMode(ObjectRegistMode mode)
        {
            _registMode = mode;
        }

        public void RegistObject(EosObjectBase obj)
        {
            RegistObject(obj, _registmodeType);
        }

        private void AddNotifyObject(EosNotifyObject obj)
        {
            _notifyobjects.Add(((EosObjectBase)obj).ObjectID);
        }

        public void ObjectAction(EosNotifyObject obj, EosObjectAction action)
        {
            _objectactionlist.Add(new Tuple<EosObjectAction, EosNotifyObject>(action,obj));
        }
        public void RegistObject(EosObjectBase obj,ObjectType objectType)
        {
            if (_objectlist.ContainsKey(obj.ObjectID))
                return;
            // if (obj is ITransform unityobj )
            //     _unitylist.Add(unityobj.Transform.gameObject.GetHashCode(),obj);
            
            obj.Notify += AddNotifyObject;
            
            if (_registMode == ObjectRegistMode.NoCreateKey)
            {
                if (obj.ObjectID != 0)
                    _objectlist.Add(obj.ObjectID, obj);
                return;
            }
            if (obj.ObjectID != 0)
                _objectlist.Add(obj.ObjectID, obj);
            else
            {
                uint okey = 0;
                    if (objectType == ObjectType.Sync && PhotonNetwork.CurrentRoom != null && Ref.SessionManager.IsServer)
                    {
                        var cp = PhotonNetwork.CurrentRoom.CustomProperties;
                        if (cp["seed"] == null)
                        {
                            cp["seed"] = (int) objectkey;
                        }
                        else
                            objectkey = Convert.ToUInt32(cp["seed"]);

                        okey = objectkey;
                        // Debug.Log($"update seed:{objectkey}");
                        
                        obj.ObjectID = okey | ((uint) objectType) << 24;
                        if (_objectlist.ContainsKey(obj.ObjectID))
                        {
                            Debug.Log($"already exist : {obj.ObjectID}");
                            return;
                        }
                        _objectlist.Add(obj.ObjectID, obj);
                        cp["seed"] = (int) objectkey + 1;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(cp);
                    }
                    else
                    {
                        obj.ObjectID = objectkey | ((uint) objectType) << 24;
                        if (_objectlist.ContainsKey(obj.ObjectID))
                        {
                            Debug.Log($"already exist : {obj.ObjectID}");
                            return;
                        }
                        _objectlist.Add(obj.ObjectID, obj);
                        objectkey++;
                    }
            }
        }
        public void UnRegistObject(EosObjectBase obj)
        {
            _objectlist.Remove(obj.ObjectID);
            _notifyobjects.Remove(obj.ObjectID);
            UnRegistUpdateObject(obj);
            OnUnRegistObject?.Invoke(this, obj);
        }
        public void Reset()
        {
            _objectlist = new Dictionary<uint, EosObjectBase>();
            _unitylist = new Dictionary<int, EosObjectBase>();
            _updateobjectlist = new List<EosObjectBase>();
            _latedeleteupdateobjectlist = new List<EosObjectBase>();
            _lateaddupdateobjectlist = new List<EosObjectBase>();
            _registmodeType = ObjectType.NonSync;
        }
        public EosObjectBase GetFromUnityObject(GameObject unityobj)
        {
            var unityhashcode = unityobj.GetHashCode();
            if (_unitylist.ContainsKey(unityhashcode))
                return _unitylist[unityhashcode];
            return null;
        }
        public void RegistUpdateObject(EosObjectBase obj)
        {
            if (_lateaddupdateobjectlist.Contains(obj))
                return;
            _lateaddupdateobjectlist.Add(obj);
        }
        public void UnRegistUpdateObject(EosObjectBase obj)
        {
            if (_latedeleteupdateobjectlist.Contains(obj))
                return;
            _latedeleteupdateobjectlist.Add(obj);
        }
        public void Update(float delta)
        {
            _objectactionlist.ForEach(a => ObjectNotifyInvoker.InvokeObjectAction(a.Item2 , a.Item1,false));
            _objectactionlist.Clear();

            if (_lateaddupdateobjectlist.Count>0)
                _lateaddupdateobjectlist.ForEach(obj => _updateobjectlist.Add(obj));
            if (_latedeleteupdateobjectlist.Count>0)
                _latedeleteupdateobjectlist.ForEach(obj => _updateobjectlist.Remove(obj));
            foreach (var it in _updateobjectlist)
                it.Update(delta);
            _lateaddupdateobjectlist.Clear();
            _latedeleteupdateobjectlist.Clear();
            if (_notifyobjects.Count > 0)
            {
                NotifyObjectPacket packet = new NotifyObjectPacket();
                foreach (var objid in _notifyobjects)
                {
                    var obj =_objectlist[objid];
                    packet.notifies.Add(obj.MakeNotifications());
                }
                var data = MessagePackSerializer.Serialize(packet);
                var decode = MessagePackSerializer.Deserialize<NotifyObjectPacket>(data);
                // if (Ref.SessionManager.IsServer)
                    Ref.SessionManager.SendPacket((int) S2C.Commands.NotifyObjectProperty,data );
                _notifyobjects.Clear();
            }
        }

        [Commands((int) S2C.Commands.CreateObject)]
        private void OnCreateObject(CmdPacket packet)
        {
            var createObject = MessagePackSerializer.Deserialize<CreateObjectPacket>(packet.data);
            Debug.Log($"data:{createObject}");
        }

        [Commands((int) S2C.Commands.NotifyObjectProperty)]
        private void OnNotifyObjectProperty(CmdPacket packet)
        {
            var notifyobjectpacket = MessagePackSerializer.Deserialize<NotifyObjectPacket>(packet.data, ContractlessStandardResolver.Options);
            foreach (var n in notifyobjectpacket.notifies)
            {
                if (!_objectlist.ContainsKey(n.objectid))
                    continue;
                var targetobject = _objectlist[n.objectid];
                foreach (var p in n.properties)
                {
                    var prop = EosNotifyCache.GetNotifyProperty(targetobject,p.notify);
                    if (prop==null)
                        continue;
                    var method = EosNotifyCache.GetNotifyActionMethod(p.notify);
                    prop.SetValue(targetobject,p.value);
                    method?.Invoke(targetobject,null);
                    ObjectNotifyInvoker.InvokePropertyChange(targetobject,(EosPropertyNotify)p.notify,true);
                }
            }
//            Debug.Log($"data:{notifyobjectpacket}");
        }

        [Commands((int) C2S.Commands.NotifyObjectAction)]
        private void OnNotifyObjectAction(CmdPacket packet)
        {
            var notifyobjectaction = MessagePackSerializer.Deserialize<NotifyObjectActionPacket>(packet.data);
            if (!_objectlist.ContainsKey(notifyobjectaction.objectid))
                return;
            var target = _objectlist[notifyobjectaction.objectid];
            var method = EosNotifyCache.GetNotifyObjectAction(target, notifyobjectaction.notifyaction);
            method?.Invoke(target, notifyobjectaction.parameters);
            ObjectNotifyInvoker.InvokeObjectAction(target,(EosObjectAction)notifyobjectaction.notifyaction,true);
        }
        public void Log()
        {
            Debug.Log("-------------- OBJECT MNG---------------------");
            foreach (var obj in _objectlist)
            {
                Debug.Log($"objid:{obj.Key} - {obj.Value.Name}");
            }
        }
    }
}
namespace EosPlayer
{
    using EosLuaPlayer;
    using Eos.Service;
    using Eos.Objects;
    using Eos.Script;

    public partial class EosPlayer : MonoBehaviour
    {
        public delegate void OnConnectedDelegate();
        public delegate void OnJoinedRoomDelegate(int localactor);
        public delegate void OnPlayerConnected(int actornum,bool islocalplayer);
        public delegate void OnPlayerDisConnected(int actornum);
        private Players _players;
        private Solution _solution;
        private ObjectManager _objectmanager = new ObjectManager();
        private ScriptPlayer _scriptPlayer = new ScriptPlayer();
        private Scheduler _scheduler = new Scheduler();
        private EosLuaPlayer _luaplayer = new EosLuaPlayer();
        private ByteLoader _byteLoader = new ByteLoader();
        private bool _isplaying;
        private static EosPlayer _instance;
        public ObjectManager ObjectManager =>_objectmanager;
        public Players Players => _players; 
        public IResource Resource { get; set; }
        public ISessionManager SessionManager { get; set; }

        public Solution Solution => _solution;
        public CoroutineManager Coroutine = null;
        public ScriptPlayer ScriptPlayer => _scriptPlayer;
        public Scheduler Scheduler => _scheduler;
        public EosLuaPlayer LuaPlayer => _luaplayer;
        public bool IsPlaying => _isplaying;
        public ByteLoader ByteLoader => _byteLoader;

        public static EosPlayer Instance
        {
            get
            {
                if (_instance ==null)
                {
                    var unityadapter = Eos.ObjectFactory.CreateUnityInstance("EosPlayer").gameObject;
                    _instance = unityadapter.AddComponent<EosPlayer>();
                    _instance.Coroutine = new CoroutineManager(_instance);
                }
                return _instance;
            }
        }
        void Awake()
        {
            IngameScriptContainer.Initialize();
            Resource = EOSResource.Instance;
            SessionManager = Photon.Pun.SessionManager.Instance;
            SessionManager.ConnectedToMaster += Connected;
            SessionManager.Connected += SessionManagerOnConnected;
            enabled = false;
        }

        private void Connected()
        {
            SessionManager.CreateOrJoinRoom("TestRoom");
        }
        private void SessionManagerOnConnected()
        {
            if (PhotonNetwork.IsConnected)
                return;
            SessionManager.JoinRoom("TestRoom");
        }

        private void Start()
        {
            // SessionManager.Instance.Connect("RoomMaster");
            // SessionManager.Instance.Connected += () => { StartCoroutine(coJoinRoom()); };
        }

        void Update()
        {
            ObjectManager.Update(Time.deltaTime);
            Scheduler.Update(Time.deltaTime);
            LuaPlayer.Update();
        }
        private void LateUpdate()
        {
            
        }
        public void Play()
        {
            _objectmanager.SetRegistType(ObjectType.Loaded);
            _players = Eos.ObjectFactory.CreateEosObject<Players>();
            
            CommandsCache.Init(this);
            SessionManager.Connect("RoomMaster");
        }
        public void Stop()
        {
            _isplaying = false;
            enabled = false;
            try
            {
                _instance?.Solution?.Destroy();
            }
            catch(Exception ex)
            {

            }
            Scheduler.Clear();
        }
        public static void ShutDown()
        {
            _instance?.Solution?.Destroy();
            _instance = null;
            
        }
        void OnApplicationQuit()
        {
            ShutDown();
        }
        public void SetSolution(Solution solution)
        {
            _solution = solution;
            _solution.Init();
        }

        public void StartGame()
        {
            _isplaying = true;
            _luaplayer.Init();
            enabled = true;
//            PhotonNetwork.CurrentRoom.CustomProperties["seed"] = 2;
            _solution?.StartGame();
        }
    }
}

namespace Photon.Pun
{
    public interface ISessionManager
    {
        bool IsServer { get; }
        int LocalActorID { get; }
        void Connect(string username);
        event EosPlayer.EosPlayer.OnConnectedDelegate Connected;
        event EosPlayer.EosPlayer.OnPlayerConnected PlayerConnected;
        event EosPlayer.EosPlayer.OnPlayerDisConnected PlayerDisConnected;
        event EosPlayer.EosPlayer.OnConnectedDelegate ConnectedToMaster;
        event EosPlayer.EosPlayer.OnJoinedRoomDelegate CreatedRoom;
        event EosPlayer.EosPlayer.OnJoinedRoomDelegate JoinedRoom;
        void CreateOrJoinRoom(string roomName);
        void JoinRoom(string roomName);
        void SendPacket(int cmd_, byte[] data);
        void SendPacket(int targetplayer,int cmd_, byte[] data);
    }
   
}
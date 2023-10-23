using System;
using System.Collections;
using System.Collections.Generic;
using Eos.Network;
using Eos.Objects.Description;
using UnityEngine;


namespace Eos.Network
{
    using  MessagePack;
    [MessagePackObject]
    public class CmdPacket
    {
        [Key(1)]
        public int cmd;
        [Key(2)]
        public byte[] data;
    }
}

namespace Photon.Pun
{
    using ExitGames.Client.Photon;
    using Realtime;

    public class SessionManager : MonoBehaviourPunCallbacks , ISessionManager, IPunObservable , IOnEventCallback
    {
        private static SessionManager _session;
        private PhotonView _photonview;
        public bool IsServer => PhotonNetwork.IsMasterClient;
        public int LocalActorID => PhotonNetwork.LocalPlayer.ActorNumber;
        public event EosPlayer.EosPlayer.OnConnectedDelegate Connected;
        public event EosPlayer.EosPlayer.OnPlayerConnected PlayerConnected;
        public event EosPlayer.EosPlayer.OnPlayerDisConnected PlayerDisConnected;
        public event EosPlayer.EosPlayer.OnConnectedDelegate ConnectedToMaster;
        public event EosPlayer.EosPlayer.OnJoinedRoomDelegate CreatedRoom;
        public event EosPlayer.EosPlayer.OnJoinedRoomDelegate JoinedRoom;
        public static SessionManager Instance =>
            _session ?? (_session = new GameObject("SessionManager").AddComponent<SessionManager>());

        private void Start()
        {
            _photonview = gameObject.AddComponent<PhotonView>();
            _photonview.ViewID = 1;
            _photonview.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
            _photonview.FindObservables();
        }

        public void RPC(string method,params object[]args)
        {
            _photonview.RPC(method,RpcTarget.Others,args);
            var data = new byte[] {1, 2, 3, 4, 5};
            //SendPacket(1024,data,4);
        }

        public void SendPacket(int cmd_, byte[] data_)
        {
            var msgdata = new CmdPacket { cmd = cmd_,data = data_};
            var data = MessagePack.MessagePackSerializer.Serialize(msgdata);
            PhotonNetwork.RaiseEvent(154, data, new RaiseEventOptions {Receivers = ReceiverGroup.Others}, SendOptions.SendReliable);
        }
        public void SendPacket(int targetplayer , int cmd_, byte[] data_)
        {
            var msgdata = new CmdPacket { cmd = cmd_,data = data_};
            var data = MessagePack.MessagePackSerializer.Serialize(msgdata);
            PhotonNetwork.RaiseEvent(154, data, new RaiseEventOptions {TargetActors = new int[]{targetplayer}}, SendOptions.SendReliable);
        }
        [PunRPC]
        public void RPCMethod(int testvalue)
        {
            Debug.Log($"RPC Metho test sucess.{testvalue}");
        }
        public override void OnConnected()
        {
            base.OnConnected();
            Debug.Log("Connected.......................");
            Connected?.Invoke();
        }
        public override void OnCreatedRoom()
        {
            var iss = IsServer;
            base.OnCreatedRoom();
            CreatedRoom?.Invoke(PhotonNetwork.LocalPlayer.ActorNumber);
            Debug.Log("Room Created.......................");
        }

        public override void OnJoinedRoom()// my player created..
        {
            base.OnJoinedRoom();
            var myplayer = PhotonNetwork.LocalPlayer;
            JoinedRoom?.Invoke(myplayer.ActorNumber);
            PlayerConnected?.Invoke(myplayer.ActorNumber,true);
            Debug.Log($"Room Joined.......................:{PhotonNetwork.LocalPlayer.NickName}");
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            ConnectedToMaster?.Invoke();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)// other player connected..
        {
            base.OnPlayerEnteredRoom(newPlayer);
            PlayerConnected?.Invoke(newPlayer.ActorNumber,false);
            Debug.Log("Player Entered Room ......................");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            PlayerDisConnected?.Invoke(otherPlayer.ActorNumber);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
        }

        public void CreateOrJoinRoom(string roomName)
        {
            byte maxPlayers;
            byte.TryParse("3", out maxPlayers);
            maxPlayers = (byte) Mathf.Clamp(maxPlayers, 2, 8);

            RoomOptions options = new RoomOptions {MaxPlayers = maxPlayers, PlayerTtl = 10000, CustomRoomProperties = new Hashtable()};

            PhotonNetwork.JoinOrCreateRoom(roomName, options, null);
        }
        public void JoinRoom(string roomName)
        {
            PhotonNetwork.JoinRoom(roomName);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            Debug.Log($"DisConnected....{cause}");
        }

        public void Connect(string username)
        {
            PhotonNetwork.LocalPlayer.NickName = username;
            PhotonNetwork.ConnectUsingSettings();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
        public void OnEvent(EventData photonEvent)
        {
            // Debug.Log($"event code:{photonEvent.Code}");
            if (photonEvent.Code == 154)
            {
                var packet = MessagePack.MessagePackSerializer.Deserialize<CmdPacket>((byte[]) photonEvent.CustomData);
                CommandsCache.Invoke(packet);
            }
        }
    }
}
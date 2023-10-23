using System;
using MessagePack;
using Photon.Pun;

namespace Eos.Objects
{
    using Ore;
    using Script;
    [System.Serializable]
    [EosObject]
    [MessagePackObject]
    public partial class EosServerScript : EosScript
    {
        protected override bool CanDo()
        {
            return Ref.SessionManager.IsServer;
        }
    }
}
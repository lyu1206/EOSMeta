using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

public interface ITransform
{
    Eos.Objects.EosTransform  Transform {get;}
}

namespace Eos.Service
{
    using Eos.Objects;
    [System.Serializable]
    [NoCreated]
    public partial class Workspace : EosService
    {
        public Workspace()
        {
            Name = "Workspace";
        }
    }
}
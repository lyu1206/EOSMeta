using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace Eos.Service
{
    using Objects;

    [NoCreated]
    [MessagePackObject]
    public partial class TypeService : EosService
    {
        public override void Activate(bool active, bool recursivechild = true)
        {
            if (!Ref.IsPlaying)
                return;
            _transform.Transform?.gameObject.SetActive(false);
        }
        public override void StartPlay()
        {
        }
    }

    [System.Serializable]
    [NoCreated]
    [RefferenceService]
    [MessagePackObject]
    public class StarterPlayer : TypeService
    {
        public StarterPlayer()
        {
            _transform.Name = "StarterPlayer";
        }
    }
    [System.Serializable]
    [NoCreated]
    [RefferenceService]
    [MessagePackObject]
    public class StarterPack : TypeService
    {
        public StarterPack()
        {
            _transform.Name = "StarterPack";
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MessagePack;
using Unity.VisualScripting;

//using Ludiq;

namespace Eos.Service
{
    using AI;
    using Objects;

    [NoCreated]
    [MessagePackObject]
    public abstract partial class EosService : EosObjectBase, ITransform
    {
        public EosService()
        {
            _transform = ObjectFactory.CreateInstance<EosTransform>();
        }
        protected EosTransform _transform;
        [IgnoreMember]public virtual EosTransform Transform => _transform;
        public override void OnDestroy()
        {
            _transform?.Destroy();
        }
        public override void OnCreate()
        {
            base.OnCreate();
            _transform.Create(Name);
        }

    }
    [System.Serializable]
    [NoCreated]
    [MessagePackObject]
    public partial class Solution : EosObjectBase
    {
        private Workspace _workspace;
        private TerrainService _terrainservice;
        private GUIService _guiservice;
        private AIService _aiservice;
        private StarterPlayer _starterplayer;
        private StarterPack _starterpack;
        [IgnoreMember] public Workspace Workspace { get => _workspace; set => _workspace = value; }
        [IgnoreMember] public TerrainService Terrain { get => _terrainservice; set => _terrainservice = value; }
        [IgnoreMember] public GUIService GUIService { get => _guiservice; set => _guiservice = value; }
        [IgnoreMember]public AIService AIService => _aiservice;
        [IgnoreMember] public StarterPlayer StarterPlayer => _starterplayer;
        [IgnoreMember] public StarterPack StarterPack => _starterpack;
        [IgnoreMember] public Players Players => Ref.Players;

        public void Init()
        {
            _aiservice = new AIService();
            _workspace = FindChild<Workspace>();
            _terrainservice = FindChild<TerrainService>();

            AddChild(Ref.Players);

            _guiservice = FindChild<GUIService>();
            _starterplayer = FindChild<StarterPlayer>();
            _starterpack = FindChild<StarterPack>();
        }
        public void StartGame()
        {
            var activelist = _children.Where(it => it.GetType().HasAttribute<RefferenceService>() == false).ToList();
            activelist.ForEach(child => child.Activate(child.Active));
            activelist.ForEach(child => child.StartPlay());
        }
    }
}

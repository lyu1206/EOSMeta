using System;
using UnityEngine;
namespace Eos.Objects
{
    using Service;
    using MessagePack;
    [Serializable]
    [MessagePackObject]
    [EosObject]
    [DescendantOf(typeof(Workspace))]
    public partial class EosCamera : EosTransformActor
    {
        private EosTransformActor _target;
        private Camera _camera;
        public EosCamera()
        {
        }
        public override void OnCreate()
        {
            base.OnCreate();
        }
        public override Vector3 LocalPosition 
        { 
            get => base.LocalPosition;
            set
            {
//                _camera.transform.localPosition = value;
                base.LocalPosition = value;
            }
        }
        public override Vector3 LocalRotation 
        { 
            get => base.LocalRotation;
            set
            {
                base.LocalRotation = value;
            }
        }
        private static EosCamera _main;
        public static Action<Camera> OnChangedMainCamera;
        [IgnoreMember]public static EosCamera Main
        {
            set
            {
                _main = value;
                OnChangedMainCamera?.Invoke(_main._camera);
            }
            get => _main;
        }
        [IgnoreMember]public EosTransformActor Target
        {
            get=>_target;
            set
            {
                _target = value;
                UnRegistComponent(update);
                if (_target != null)
                    RegistComponent(update);
            }
        }
        private void update(object sender,float delta)
        {
            var targetpos = _target.WorldPosition + new Vector3(0, 10 * 10, -10 * 10);
            LocalPosition = targetpos;
            _transform.Transform.LookAt(_target.Transform.Transform, Vector3.up);
        }
        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            _camera = _transform.GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = _transform.Transform.gameObject.AddComponent<Camera>();
                Camera.SetupCurrent(_camera);
            }
            var camera = _camera;// = _transform.Transform.gameObject.AddComponent<Camera>();
            camera.name = Name;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
//            Camera.main.enabled = false;
            camera.enabled = true;

            Main = this;
        }
        public Vector3 CameraspaceDirection(Vector3 vec)
        {
            return _camera.transform.TransformDirection(vec);
        }
        public void LookAt(EosTransformActor target)
        {
            Transform.Transform.LookAt(target.Transform.Transform.position);
        }
    }
}
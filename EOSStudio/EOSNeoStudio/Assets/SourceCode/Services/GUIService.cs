using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MessagePack;

namespace Eos.Service
{
    using Eos.Objects;
    using EosPlayer;
    [Serializable]
    [NoCreated]
    public partial class GUIService : EosService
    {
        private Canvas _canvas;
        private Transform _root;
        [IgnoreMember] public Canvas Canvas => _canvas;
        public GUIService()
        {
            Name = "GUIRoot";
        }
        private void CreateCanvas()
        {
            _canvas = _transform.GetComponent<Canvas>();
            if (_canvas != null)
                return;
            _canvas = _transform.AddComponent<Canvas>();
            var scaler = _transform.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 1280);
            _transform.AddComponent<GraphicRaycaster>();

            _root = _transform.Transform;

            if (EventSystem.current == null)
            {
                var eventsystem = ObjectFactory.CreateUnityInstance("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).gameObject;
                eventsystem.transform.parent = _root.transform;
            }
            //            _guiroot.Transform = root.transform;
        }
        protected override void OnActivate(bool active)
        {
            base.OnActivate(active);
            if (_canvas != null)
                return;
            CreateCanvas();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        //public UGUIEvents RegistUIEvent(Transform eventobject,object reciever)
        //{
            
        //    var uguievent = eventobject.gameObject.AddComponent<UGUIEvents>();
        //    return uguievent;
        //}
        //public void UnRegistUIEvent(UGUIEvents uievent)
        //{
        //    if (uievent != null)
        //        GameObject.Destroy(uievent);
        //}
    }
}

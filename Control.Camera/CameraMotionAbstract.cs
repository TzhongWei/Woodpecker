using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public abstract class CameraMotionAbstract : ICameraMotion
    {
        protected bool _applyCameraMotion = false;
        protected RhinoView vp;
        protected RhinoViewport avp;
        public CameraMotionAbstract() { }
        public CameraMotionAbstract(CameraParameter keyCamera, Interval timeline)
        {
            this.KeyCamera = keyCamera;
            this.timeline = timeline;
            this.MotionCamera = keyCamera.Duplicate(keyCamera.Name + "_C", false);
        }
        private CameraParameter _keyCamera;
        public CameraParameter KeyCamera
        {
            get => _keyCamera; set
            {
                if (value == null)
                    throw new Exception("KeyCamera cannot be null.");
                //if (value.SourceType != CameraReference.RhinoReference)
                //    throw new Exception("KeyCamera must be RhinoReference.");

                _keyCamera = value;
            }
        }
        public CameraParameter MotionCamera { get; set; }
        public Interval timeline { get; set; }
        public virtual void Initialised()
        {
            this.MotionCamera = this.KeyCamera.Duplicate(this.KeyCamera.Name + "_C", false);
        }
        public virtual bool PreEvaluate()
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return false;
            vp = doc.Views.ActiveView;
            if (vp == null) return false;
            avp = vp.ActiveViewport;
            if (avp == null) return false;
            this.MotionCamera = this.KeyCamera.Duplicate(this.KeyCamera.Name + "_C", false);

            if (!_applyCameraMotion) return true;

            // If applyCameraMotion is true, we will push the current camera state to the named view corresponding to the KeyCamera's ViewIndex. This allows us to update the camera in Rhino based on the motion's transformations. If applyCameraMotion is false, we will not modify the camera in Rhino and just return the evaluated camera parameters without affecting the viewport. This gives us flexibility to either apply the motion directly to the Rhino viewport or just use it for calculations without modifying the view.
            if (KeyCamera.SourceType == CameraReference.RhinoReference &&
                KeyCamera.ViewIndex >= 0 &&
                KeyCamera.ViewIndex < doc.NamedViews.Count)
            {
                avp.PushViewInfo(
                    doc.NamedViews[KeyCamera.ViewIndex],
                    false);
            }
            else
            {
                // Initialize directly from the phantom camera snapshot.
                avp.SetViewProjection(
                    KeyCamera.viewportInfo,
                    false);

                // Parallel zoom is currently stored separately.
                if (KeyCamera.IsParallel)
                    avp.ZoomWindow(KeyCamera.WindowRect);
            }


            return true;
        }
        public void SetApplyCameraMotion(bool apply)
        {
            _applyCameraMotion = apply;
        }
        public abstract CameraParameter Evaluate(double t);
        public virtual bool PostEvaluate()
        {
            if (_applyCameraMotion)
                vp.Redraw();
            return true;
        }
        public abstract void ApplyMotion(bool IsFinished);
        public bool IsInclude(double t)
        {
            return this.timeline.IncludesParameter(t);
        }
        public bool IsEnd(double t)
        {
            return t >= this.timeline.Max;
        }
    }
}
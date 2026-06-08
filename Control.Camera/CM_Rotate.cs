using System;
using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_Rotate : CameraMotionAbstract
    {
        private double _radians;
        private Vector3d _axis;
        public CM_Rotate(CameraParameter keyCamera, double radians, Vector3d axis) : base(keyCamera, new Interval(0, 1))
        {
            _radians = radians;
            _axis = axis;
        }
        public CM_Rotate(CameraParameter keyCamera, double radians, Vector3d axis, Interval timeline) : base(keyCamera, timeline)
        {
            _radians = radians;
            _axis = axis;
        }
        public override CameraParameter Evaluate(double t)
        {
            var newCam = this.MotionCamera;
            double factor;
            if (t <= timeline.Min)
            {
                factor = 0.0;
            }
            else if (t >= timeline.Max)
            {
                factor = 1.0;
            }
            else
            {
                factor = (t - timeline.Min) / timeline.Length;
                factor = Math.Max(0, Math.Min(1, factor)); // Clamp factor to [0, 1]
            }
            var newAngle = _radians * factor;
            CameraTransform.Rotate(ref newCam, newAngle, _axis);

            this.MotionCamera = newCam;

            // if (this._applyCameraMotion)
            // {
            //     if (newCam.IsParallel)
            //     {
            //         avp.ZoomWindow(newCam.WindowRect);
            //     }
            //     else
            //     {
            //         avp.Camera35mmLensLength = newCam.CameraLength;
            //     }
            //     avp.SetCameraLocations(newCam.CameraTarget, newCam.CameraLocation);
            //     avp.Name = "Motion";
            //     avp.CameraUp = newCam.CameraUp;
            // }

            return newCam;
        }
        public override void ApplyMotion(bool IsFinished)
        {
            
            avp.SetCameraLocations(this.MotionCamera.CameraTarget, this.MotionCamera.CameraLocation);
            avp.CameraUp = this.MotionCamera.CameraUp;
            if (this.MotionCamera.IsParallel)
            {
                avp.ZoomWindow(this.MotionCamera.WindowRect);
            }
            else
            {
                avp.Camera35mmLensLength = this.MotionCamera.CameraLength;
            }
            avp.Name = IsFinished ? "Rotate_Finished" : "Motion";
        }
    }
}
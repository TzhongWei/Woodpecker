using System;
using Rhino;
using Rhino.Geometry;
using Rhino.UI.Internal.OptionsPages;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_Dolly : CameraMotionAbstract
    {
        private double _distance;
        private CM_Dolly() { }
        public CM_Dolly(CameraParameter keyCamera, double Distance) : base(keyCamera, new Interval(0, 1))
        {
            this._distance = Distance;
        }
        public CM_Dolly(CameraParameter keyCamera, Interval timeInterval, double Distance) : base(keyCamera, timeInterval)
        {
            this._distance = Distance;
        }

        public override CameraParameter Evaluate(double t)
        {
            var newCam = this.MotionCamera;  // Reference to the MotionCamera which is a duplicate of KeyCamera

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

            // Linear interpolation of camera distance for dolly motion.
            var newDist = _distance * factor;
            CameraTransform.Dolly(ref newCam, newDist);
            this.MotionCamera = newCam;
            
            // if (this._applyCameraMotion)
            // {
            //     if (newCam.IsParallel)
            //     {
            //         avp.ZoomWindow(newCam.WindowRect);
            //     }
            //     else
            //     {
            //         avp.SetCameraLocations(newCam.CameraTarget, newCam.CameraLocation);
            //         avp.Camera35mmLensLength = newCam.CameraLength;
            //     }
            //     avp.Name = "Motion";
            //     avp.CameraUp = newCam.CameraUp;
            // }

            return newCam;
        }
        public override void ApplyMotion(bool IsFinished)
        {
            avp.CameraUp = this.MotionCamera.CameraUp;
            if (this.MotionCamera.IsParallel)
            {
                ZoomParallelWindows(this);
            }
            else
            {
                avp.SetCameraLocations(this.MotionCamera.CameraTarget, this.MotionCamera.CameraLocation);
                avp.Camera35mmLensLength = this.MotionCamera.CameraLength;
            }
            avp.Name = IsFinished ? "Dolly_Finished" : "Motion";
        }
    }
}

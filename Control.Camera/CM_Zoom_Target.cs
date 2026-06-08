using System;
using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_Zoom_Target : CameraMotionAbstract
    {
        private double _factor;
        public double Factor
        {
            get => _factor;
            set
            {
                _factor = value == 0 ? 1 : value;
            }
        }
        public Point3d Target;
        private CM_Zoom_Target() { }
        public CM_Zoom_Target(CameraParameter keyCamera, double Factor, Point3d Target) : base(keyCamera, new Interval(0, 1))
        {
            this.Factor = Factor;
            this.Target = Target;
        }
        public CM_Zoom_Target(CameraParameter keyCamera, Interval timeline, double Factor, Point3d Target) : base(keyCamera, timeline)
        {
            this.Factor = Factor;
            this.Target = Target;
        }
        public override CameraParameter Evaluate(double t)
        {
            var newCam = this.MotionCamera;
            double sFactor;
            if (t <= timeline.Min)
            {
                sFactor = 1;
            }
            else if (t >= timeline.Max)
            {
                sFactor = Factor;
            }
            else
            {
                t = (t - timeline.Min) / timeline.Length;
                t = Math.Max(0, Math.Min(1, t));
                sFactor = 1.0 + t * (Factor - 1.0);
            }

            CameraTransform.Zoom(ref newCam, sFactor, Target);

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
                avp.ZoomWindow(this.MotionCamera.WindowRect);
            }
            else
            {
                avp.SetCameraLocations(this.MotionCamera.CameraTarget, this.MotionCamera.CameraLocation);
                avp.Camera35mmLensLength = this.MotionCamera.CameraLength;
            }
            avp.Name = IsFinished ? "Zoom_Finished" : "Motion";
            
        }
    }
}
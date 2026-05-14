using System;
using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_Zoom : CameraMotionAbstract
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
        private CM_Zoom(){}
        public CM_Zoom(CameraParameter keyCamera, double Factor): base(keyCamera, new Interval(0,1))
        {
            this.Factor = Factor;
        }
        public CM_Zoom(CameraParameter keyCamera, Interval timeline, double Factor): base(keyCamera, timeline)
        {
            this.Factor = Factor;
        }
        public override CameraParameter Evaluate(double t)
        {
            var newCam = this.MotionCamera;
            double sFactor;
            if(t <= timeline.Min)
            {
                sFactor = 1;
            }
            else if(t >= timeline.Max)
            {
                sFactor = Factor;
            }
            else
            {
                t = (t - timeline.Min) / timeline.Length;
                t = Math.Max(0, Math.Min(1, t));
                sFactor = 1.0 + t * (Factor - 1.0);
            }

            CameraTransform.Zoom(ref newCam, sFactor);

            if(this._applyCameraMotion)
            {
                if(newCam.IsParallel)
                {
                    avp.ZoomWindow(newCam.WindowRect);
                }
                else
                {
                    avp.SetCameraLocations(newCam.CameraTarget, newCam.CameraLocation);
                    avp.Camera35mmLensLength = newCam.CameraLength;
                }
                avp.Name = "Motion";
                avp.CameraUp = newCam.CameraUp;
            }

            return newCam;
        }
    }
}
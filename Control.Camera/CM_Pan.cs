using System;
using Rhino.Geometry;
using Rhino.Render.Fields;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_Pan : CameraMotionAbstract
    {
        private Vector3d _panVector;
        public CM_Pan ResetVectorLength(double Factor)
        {
            _panVector.Unitize();
            _panVector *= Factor;
            return this;
        }
        public CM_Pan(CameraParameter keyCamera, Vector3d panVector) : base(keyCamera, new Interval(0, 1))
        {
            _panVector = panVector;
        }
        public CM_Pan(CameraParameter keyCamera, Vector3d panVector, double Factor) : base(keyCamera, new Interval(0, 1))
        {
            panVector.Unitize();
            _panVector = panVector * Factor;
        }
        public CM_Pan(CameraParameter keyCamera, Vector3d panVector, Interval timeline) : base(keyCamera, new Interval(0, 1))
        {
            _panVector = panVector;
        }
        public override CameraParameter Evaluate(double t)
        {
            var newCam = this.MotionCamera;
            Vector3d panfactor;
            if (t <= timeline.Min)
            {
                panfactor = new Vector3d(0, 0, 0);
            }
            else if (t >= timeline.Max)
            {
                panfactor = this._panVector;
            }
            else
            {
                t = (t - timeline.Min) / timeline.Length;
                t = Math.Max(0, Math.Min(t, 1));
                panfactor = t * _panVector;
            }

            CameraTransform.Pan(ref newCam, panfactor);

            if (this._applyCameraMotion)
            {
                if (newCam.IsParallel)
                {
                    avp.ZoomWindow(newCam.WindowRect);
                }
                else
                {
                    avp.Camera35mmLensLength = newCam.CameraLength;
                }
                avp.SetCameraLocations(newCam.CameraTarget, newCam.CameraLocation);
                avp.Name = "Motion";
                avp.CameraUp = newCam.CameraUp;

            }
            return newCam;
        }
    }
}
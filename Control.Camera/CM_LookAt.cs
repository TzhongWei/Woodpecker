using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_LookAt : CameraMotionAbstract
    {
        private double _lensLength;
        private Point3d _target;
        private Vector3d _upDirection;
        public CM_LookAt(CameraParameter keyCamera, Point3d TargetPt, Vector3d UpDirection, double lensLength = -1) : base(keyCamera, new Interval(0, 1))
        {
            _lensLength = lensLength <= 0 ? keyCamera.CameraLength : lensLength;
            _target = TargetPt;
            if (UpDirection.Unitize())
                _upDirection = UpDirection;
            else
                _upDirection = keyCamera.CameraUp;
        }
        public CM_LookAt(CameraParameter keyCamera, Point3d TargetPt, Vector3d UpDirection, double lensLength, Interval timeline) : base(keyCamera, timeline)
        {
            _lensLength = lensLength <= 0 ? keyCamera.CameraLength : lensLength;
            _target = TargetPt;
            if (UpDirection.Unitize())
                _upDirection = UpDirection;
            else
                _upDirection = keyCamera.CameraUp;
        }
        public override CameraParameter Evaluate(double t)
        {
            var newCam = this.MotionCamera;
            if (t >= timeline.Max)
            {
                CameraTransform.LookAt(ref newCam, _target, _upDirection, _lensLength);
            }

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
            
            avp.Name = IsFinished ? "LookAt_End" : "Motion";
        }
    }
}
using Rhino.Geometry;
using System;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_Orbit : CameraMotionAbstract
    {
        private double _angleDegrees;
        private Point3d centre;
        private Vector3d axis = Vector3d.ZAxis; // Default to Z-axis, can be modified to allow custom axis of rotation
        public CM_Orbit() { }
        public CM_Orbit(CameraParameter keyCamera, double AngleDegrees, Vector3d Axis, Point3d Centre) : base(keyCamera, new Interval(0, 1))
        {
            this._angleDegrees = AngleDegrees;
            this.centre = Centre;
            this.axis = Axis;
        }
        public CM_Orbit(CameraParameter keyCamera, Interval timeInterval, double AngleDegrees, Vector3d Axis, Point3d Centre) : base(keyCamera, timeInterval)
        {
            this._angleDegrees = AngleDegrees;
            this.centre = Centre;
            this.axis = Axis;
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

            // Linear interpolation of camera angle for orbit motion.
            var newAngle = _angleDegrees * factor;
            CameraTransform.Orbit(ref newCam, newAngle, axis, centre);

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
            avp.Name = IsFinished ? "Orbit_Finished" : "Motion";
            
        }
    }
}
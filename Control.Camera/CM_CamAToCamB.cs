using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.Control.Camera
{
    public class CM_CamAToCamB : CameraMotionAbstract, IMultiCamsTransform
    {
        public List<CameraMotionAbstract> CamerasTS { get; private set; }
        private CameraParameter keyCam2;
        public CM_CamAToCamB(CameraParameter keyCamera1, CameraParameter keyCamera2) : base(keyCamera1, new Interval(0, 1))
        {
            keyCam2 = keyCamera2.Duplicate(keyCamera2.Name + "_B", false);
            if (keyCam2.IsParallel != this.KeyCamera.IsParallel)
            {
                keyCam2.SetParallel(keyCamera1.IsParallel);
            }
            this.CamerasTS = new List<CameraMotionAbstract>();

        }
        public CM_CamAToCamB(CameraParameter keyCamera1, CameraParameter keyCamera2, List<CameraMotionAbstract> cameras) : base(keyCamera1, new Interval(0, 1))
        {
            keyCam2 = keyCamera2.Duplicate(keyCamera2.Name + "_B", false);
            this.CamerasTS = new List<CameraMotionAbstract>();
            var tempCams = new List<CameraMotionAbstract>();
            if (keyCam2.IsParallel != this.KeyCamera.IsParallel)
            {
                keyCam2.SetParallel(keyCamera1.IsParallel);
            }
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].KeyCamera = keyCamera1;
                tempCams.Add(cameras[i]);
            }
            double min = 0, max = 0;
            TimelineSetting.IntervalRange(tempCams.Select(x => x.timeline).ToList(), ref min, ref max);

            // Remap all camera-motion timelines into the normalized [0, 1] range used by CamA-to-CamB.
            // The input camera motions may have their own local timeline ranges. Since this motion evaluates with
            // a normalized t value, each child motion timeline is shifted and scaled into the same global range.
            var range = max - min;
            if (range <= 1e-9)
            {
                for (int i = 0; i < tempCams.Count; i++)
                {
                    tempCams[i].timeline = new Interval(0, 1);
                    this.CamerasTS.Add(tempCams[i]);
                }
            }
            else
            {
                for (int i = 0; i < tempCams.Count; i++)
                {
                    var tl = tempCams[i].timeline;
                    var remappedMin = (tl.Min - min) / range;
                    var remappedMax = (tl.Max - min) / range;
                    remappedMin = Math.Max(0.0, Math.Min(1.0, remappedMin));
                    remappedMax = Math.Max(0.0, Math.Min(1.0, remappedMax));

                    if (remappedMax < remappedMin)
                    {
                        var tmp = remappedMin;
                        remappedMin = remappedMax;
                        remappedMax = tmp;
                    }

                    tempCams[i].timeline = new Interval(remappedMin, remappedMax);
                    this.CamerasTS.Add(tempCams[i]);
                }
            }
        }

        public override CameraParameter Evaluate(double t)
        {
            var keyCam1 = this.KeyCamera.Duplicate("CamA", false);
            var newCam = this.MotionCamera;

            t = Math.Max(0, Math.Min(t, 1)); // Clamp t to [0, 1]
            if (t <= 1e-6)
            {
                newCam = this.MotionCamera;
            }
            else if (t >= 1 - 1e-6)
            {
                newCam = this.keyCam2.Duplicate(keyCam2.Name + "_End", false);
            }
            else
            {
                var CurrentCamLoc = CameraUtil.Lerp(keyCam1.CameraLocation, keyCam2.CameraLocation, t);
                var CurrentCamTarget = CameraUtil.Lerp(keyCam1.CameraTarget, keyCam2.CameraTarget, t);
                var CurrentCamUp = CameraUtil.Lerp(keyCam1.CameraUp, keyCam2.CameraUp, t);
                CurrentCamUp.Unitize();
                var CurrentCamLength = CameraUtil.Lerp(keyCam1.CameraLength, keyCam2.CameraLength, t);

                newCam.viewportInfo.SetCameraLocation(CurrentCamLoc);
                var newDir = CurrentCamTarget - CurrentCamLoc;
                if (!newDir.Unitize())
                {
                    newDir = keyCam1.CameraDirection;
                }
                newCam.viewportInfo.SetCameraDirection(newDir);
                newCam.viewportInfo.SetCameraUp(CurrentCamUp);
                newCam.viewportInfo.Camera35mmLensLength = CurrentCamLength;
                newCam.viewportInfo.TargetPoint = CurrentCamTarget;

                if (keyCam1.IsParallel)
                {
                    newCam.WindowRect = CameraUtil.InterpolateRectangle(keyCam1.WindowRect, keyCam2.WindowRect, t);
                }

            }

            this.MotionCamera = newCam;
            newCam = EvaluateMulti(t);
            this.MotionCamera = newCam;

            if (this._applyCameraMotion)
            {
                if (keyCam1.IsParallel)
                {
                    avp.SetCameraLocations(newCam.CameraTarget, newCam.CameraLocation);
                    avp.CameraUp = newCam.CameraUp;
                    avp.ZoomWindow(newCam.WindowRect);
                }
                else
                {
                    avp.Camera35mmLensLength = newCam.CameraLength;
                    avp.SetCameraLocations(newCam.CameraTarget, newCam.CameraLocation);
                    avp.CameraUp = newCam.CameraUp;
                }
                avp.Name = "Motion";
            }


            return newCam;

        }
        public CameraParameter EvaluateMulti(double t)
        {
            if (CamerasTS.Count == 0)
                return this.MotionCamera;

            foreach (var cams in this.CamerasTS)
            {
                if (cams.IsInclude(t))
                {
                    t = Math.Max(0, Math.Min(t, 1));
                    cams.MotionCamera = this.MotionCamera; //Current motion
                    this.MotionCamera = cams.Evaluate(t);
                }
            }
            return this.MotionCamera;
        }
    }
}

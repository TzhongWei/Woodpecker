using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public class KeyCameraFrames
    {
        /// <summary>
        /// The normalised t => t [0,1]
        /// </summary>
        private readonly double _nor_t;
        public double Nor_t => _nor_t;
        private readonly Point3d _lookAt;
        public Point3d LookAt => _lookAt;
        private Vector3d _cameraUP;
        public Vector3d CameraUp => _cameraUP;
        private double _zoomfactor = 1;
        public double ZoomFactor
        {
            get => _zoomfactor;
            set
            {
                _zoomfactor =
                    value > 0 &&
                    !double.IsNaN(value) &&
                    !double.IsInfinity(value)
                        ? value
                        : 1;
            }
        }
        public KeyCameraFrames(double normalised_t, Point3d? LookAt = null, double? ZoomFactor = 1, Vector3d? cameraUp = null)
        {
            this._nor_t = Math.Max(0.0, Math.Min(1.0, normalised_t));
            this._lookAt = LookAt ?? Point3d.Unset;
            this.ZoomFactor = ZoomFactor ?? 1;
            this._cameraUP = cameraUp ?? Vector3d.Unset;
        }
        public bool TryGetDirection(
            Point3d cameraLocation,
            out Vector3d direction)
        {
            direction = Vector3d.Unset;
            if (!_lookAt.IsValid || !cameraLocation.IsValid)
                return false;

            direction = _lookAt - cameraLocation;
            return direction.IsValid && direction.Unitize();
        }
    }
    public class CM_CamMoveAlongCrv : CameraMotionAbstract
    {
        private readonly Curve _crv;
        public readonly List<KeyCameraFrames> keyCamerasFrames;
        public CM_CamMoveAlongCrv(CameraParameter keyCamera, Curve curve, Interval? timeline = null) : base(keyCamera, timeline?? new Interval(0,1))
        {
            this._crv = ValidateAndDuplicateCurve(curve);
            this.keyCamerasFrames = new List<KeyCameraFrames>();
        }
        public CM_CamMoveAlongCrv(CameraParameter keyCamera, Curve curve, List<KeyCameraFrames> keyCameraFrames, Interval? timeline = null) : base(keyCamera, timeline?? new Interval(0,1))
        {
            this._crv = ValidateAndDuplicateCurve(curve);
            this.keyCamerasFrames = keyCameraFrames?
                .Where(frame => frame != null)
                .OrderBy(frame => frame.Nor_t)
                .ToList() ?? new List<KeyCameraFrames>();
        }
        public List<CameraParameter> ShowAllKeyFrames()
        {
            var cameras = new List<CameraParameter>();
            var frames = BuildEvaluationFrames();

            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                var cameraLocation = GetCameraLocation(frame.Nor_t);
                if (!cameraLocation.IsValid)
                    continue;

                var cameraDirection = GetFrameDirection(
                    frame,
                    cameraLocation);
                var cameraUp = GetFrameCameraUp(frame);
                var previewCamera = this.KeyCamera.Duplicate(
                    $"{this.KeyCamera.Name}_KeyFrame_{i:000}",
                    false);

                bool success;
                if (previewCamera.IsParallel)
                {
                    success = CameraTransform.SetCameraPoseParallel(
                        ref previewCamera,
                        cameraLocation,
                        cameraUp,
                        cameraDirection,
                        frame.ZoomFactor);
                }
                else
                {
                    success = CameraTransform.SetCameraPosePerspective(
                        ref previewCamera,
                        cameraLocation,
                        cameraUp,
                        cameraDirection,
                        this.KeyCamera.CameraLength * frame.ZoomFactor);
                }

                if (success)
                    cameras.Add(previewCamera);
            }

            return cameras;
        }
        public override CameraParameter Evaluate(double t)
        {
            var localT = GetLocalT(t);
            var cameraLocation = GetCameraLocation(localT);

            if (!cameraLocation.IsValid)
                return this.MotionCamera;

            SampleKeyFrames(
                localT,
                cameraLocation,
                out var cameraDirection,
                out var cameraUp,
                out var zoomFactor);

            var newCam = this.KeyCamera.Duplicate(
                this.KeyCamera.Name + "_CurveMotion",
                false);

            bool success;
            if (newCam.IsParallel)
            {
                success = CameraTransform.SetCameraPoseParallel(
                    ref newCam,
                    cameraLocation,
                    cameraUp,
                    cameraDirection,
                    zoomFactor);
            }
            else
            {
                success = CameraTransform.SetCameraPosePerspective(
                    ref newCam,
                    cameraLocation,
                    cameraUp,
                    cameraDirection,
                    this.KeyCamera.CameraLength * zoomFactor);
            }

            if (success)
                this.MotionCamera = newCam;

            return this.MotionCamera;
        }

        private static Curve ValidateAndDuplicateCurve(Curve curve)
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));
            if (!curve.IsValid)
                throw new ArgumentException(
                    "The camera path must be a valid curve.",
                    nameof(curve));

            var duplicate = curve.DuplicateCurve();
            if (duplicate == null)
                throw new ArgumentException(
                    "The camera path could not be duplicated.",
                    nameof(curve));

            return duplicate;
        }

        private double GetLocalT(double t)
        {
            if (timeline.Length <= 1e-9)
                return t >= timeline.Max ? 1.0 : 0.0;

            return Math.Max(
                0.0,
                Math.Min(1.0, (t - timeline.Min) / timeline.Length));
        }

        private void SampleKeyFrames(
            double localT,
            Point3d cameraLocation,
            out Vector3d direction,
            out Vector3d cameraUp,
            out double zoomFactor)
        {
            var frames = BuildEvaluationFrames();

            if (frames.Count == 1)
            {
                direction = GetFrameDirection(
                    frames[0],
                    cameraLocation);
                cameraUp = GetFrameCameraUp(frames[0]);
                zoomFactor = frames[0].ZoomFactor;
                return;
            }

            var upperIndex = frames.FindIndex(frame => frame.Nor_t >= localT);
            if (upperIndex <= 0)
            {
                direction = GetFrameDirection(
                    frames[0],
                    cameraLocation);
                cameraUp = GetFrameCameraUp(frames[0]);
                zoomFactor = frames[0].ZoomFactor;
                return;
            }
            if (upperIndex < 0)
            {
                var last = frames[frames.Count - 1];
                direction = GetFrameDirection(
                    last,
                    cameraLocation);
                cameraUp = GetFrameCameraUp(last);
                zoomFactor = last.ZoomFactor;
                return;
            }

            var frameA = frames[upperIndex - 1];
            var frameB = frames[upperIndex];
            var span = frameB.Nor_t - frameA.Nor_t;
            var frameT = span <= 1e-9
                ? 1.0
                : (localT - frameA.Nor_t) / span;

            var directionA = GetFrameDirection(
                frameA,
                cameraLocation);
            var directionB = GetFrameDirection(
                frameB,
                cameraLocation);

            direction = CameraUtil.Lerp(directionA, directionB, frameT);
            if (!direction.Unitize())
                direction = directionA;

            var cameraUpA = GetFrameCameraUp(frameA);
            var cameraUpB = GetFrameCameraUp(frameB);
            cameraUp = CameraUtil.Lerp(
                cameraUpA,
                cameraUpB,
                frameT);
            if (!cameraUp.Unitize())
                cameraUp = cameraUpA;

            zoomFactor = CameraUtil.Lerp(
                frameA.ZoomFactor,
                frameB.ZoomFactor,
                frameT);
        }

        private List<KeyCameraFrames> BuildEvaluationFrames()
        {
            var frames = keyCamerasFrames
                .Where(frame => frame != null)
                .OrderBy(frame => frame.Nor_t)
                .ToList();

            if (frames.Count == 0 || frames[0].Nor_t > 1e-9)
            {
                frames.Insert(
                    0,
                    new KeyCameraFrames(
                        0,
                        null,
                        1));
            }

            if (frames[frames.Count - 1].Nor_t < 1.0 - 1e-9)
            {
                frames.Add(
                    new KeyCameraFrames(
                        1,
                        null,
                        1));
            }

            return frames;
        }

        private Point3d GetCameraLocation(double normalizedT)
        {
            normalizedT = Math.Max(
                0.0,
                Math.Min(1.0, normalizedT));

            double curveParameter;
            if (!_crv.NormalizedLengthParameter(
                normalizedT,
                out curveParameter))
            {
                curveParameter =
                    _crv.Domain.ParameterAt(normalizedT);
            }

            var curvePoint = _crv.PointAt(curveParameter);
            if (!curvePoint.IsValid)
                return Point3d.Unset;

            return this.KeyCamera.CameraLocation +
                (curvePoint - _crv.PointAtStart);
        }

        private Vector3d GetFrameDirection(
            KeyCameraFrames frame,
            Point3d cameraLocation)
        {
            if (frame != null &&
                frame.TryGetDirection(
                    cameraLocation,
                    out var direction))
            {
                return direction;
            }

            var fallback = this.KeyCamera.CameraDirection;
            if (!fallback.Unitize())
                fallback = Vector3d.ZAxis;

            return fallback;
        }

        private Vector3d GetFrameCameraUp(KeyCameraFrames frame)
        {
            var cameraUp = frame?.CameraUp ?? Vector3d.Unset;
            if (cameraUp.IsValid && cameraUp.Unitize())
                return cameraUp;

            cameraUp = this.KeyCamera.CameraUp;
            if (!cameraUp.Unitize())
                cameraUp = Vector3d.YAxis;

            return cameraUp;
        }

        public override void ApplyMotion(bool IsFinished)
        {
            avp.SetCameraLocations(this.MotionCamera.CameraTarget, this.MotionCamera.CameraLocation);
            avp.CameraUp = this.MotionCamera.CameraUp;
            if (this.MotionCamera.IsParallel)
            {
                ZoomParallelWindows(this);
            }
            else
            {
                avp.Camera35mmLensLength = this.MotionCamera.CameraLength;
            }
            avp.Name = IsFinished ? "CrvMove_Finished" : "Motion";
        }
    }
}

using System;
using System.Collections.Generic;
using Grasshopper.GUI;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Woodpecker.Animation.Control.Camera
{
    public static class CameraTransform
    {
        private static CameraParameter DuplicateCameraParameter(CameraParameter cameraParameter, string FunctionName)
        {
            if (cameraParameter == null)
                throw new ArgumentNullException(nameof(cameraParameter), $"CameraParameter cannot be null in {FunctionName}");
            if (cameraParameter.SourceType != CameraReference.RhinoReference && cameraParameter.SourceType != CameraReference.Phantom)
                throw new ArgumentException($"CameraParameter SourceType must be either RhinoReference or Phantom in {FunctionName}");
            if (cameraParameter.SourceType == CameraReference.RhinoReference)
            {
                // For RhinoReference cameras, we create a duplicate of the camera parameter to avoid modifying the original 
                // camera parameter that is linked to the named view in Rhino. This allows us to perform transformations on 
                // the duplicate camera parameter without affecting the original camera parameter, which can be used as a reference 
                // for other motions or transformations. The duplicate camera parameter will have a modified name to indicate 
                // that it is a transformed version of the original camera parameter, and it will not be linked to any named view 
                // in Rhino, so it can be freely modified and transformed without affecting any existing views in Rhino.
                return cameraParameter.Duplicate(cameraParameter.Name + ".P_" + FunctionName, false);
            }
            return cameraParameter;
        }
        /// <summary>
        /// Dolly:
        /// - Parallel: ViewRect inflate. <br/>
        /// - Perspective: CameraLocation + CameraTarget move. <br/>
        /// Dolly moves the camera forward or backward along its viewing direction. In a perspective projection, this is achieved by moving both the camera location and the camera target point in the direction of the camera's view vector. In a parallel projection, since moving the camera does not change the view, we instead simulate a dolly effect by inflating or deflating the view rectangle, which effectively zooms in or out while keeping the camera position fixed. The distance parameter controls how much to move the camera or how much to inflate/deflate the view rectangle, with positive values moving forward (zooming in) and negative values moving backward (zooming out).
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Dolly(ref CameraParameter camera, double distance)
        {
            CameraParameter _camera = DuplicateCameraParameter(camera, "Dolly");

            // In parallel projection, forward camera movement has little visible effect.
            // Therefore Dolly is interpreted as changing the view window size.
            if (_camera.IsParallel) //Zoom in by pixel
            {
                if (!CameraUtil.GetWorldToScreenScale(_camera.viewportInfo, out var pixelsPerUnit))
                    return false;
                var viewRect = CameraUtil.ViewRect(_camera.viewportInfo);
                if (pixelsPerUnit <= 0 || double.IsNaN(pixelsPerUnit) || double.IsInfinity(pixelsPerUnit))
                    pixelsPerUnit = 1;
                var pixelScale = (int)-Math.Round(distance * pixelsPerUnit);
                viewRect.Inflate(pixelScale, pixelScale);
                if(_camera.SourceType == CameraReference.RhinoReference)
                    _camera.WindowRect = viewRect;
                camera = _camera;
                return true;
            }
            else
            {
                var camDir = new Vector3d(_camera.CameraDirection);
                if (!camDir.Unitize())
                    return false;
                var newCamLocation = _camera.CameraLocation + camDir * distance;
                var newCamTarget = _camera.CameraTarget + camDir * distance;

                _camera.viewportInfo.SetCameraLocation(newCamLocation);
                _camera.viewportInfo.TargetPoint = newCamTarget;
                camera = _camera;
                return true;
            }
        }
        public static bool Zoom(ref CameraParameter camera, double factor, Point3d target)
        {
            var _camera = DuplicateCameraParameter(camera, "Zoom");
            if (!target.IsValid)
            {
                return Zoom(ref camera, factor);
            }

            factor = factor <= 0 ? 1 : factor;
            if (_camera.IsParallel)
            {
                var doc = RhinoDoc.ActiveDoc;
                if (doc == null || doc.Views.ActiveView == null)
                    return false;

                var viewRect = camera.SourceType == CameraReference.RhinoReference ?
                CameraUtil.ViewRect(_camera.viewportInfo) : camera.WindowRect;

                var targetScreen = doc.Views.ActiveView.ActiveViewport.WorldToClient(target);

                var left = targetScreen.X + (viewRect.Left - targetScreen.X) / factor;
                var right = targetScreen.X + (viewRect.Right - targetScreen.X) / factor;
                var top = targetScreen.Y + (viewRect.Top - targetScreen.Y) / factor;
                var bottom = targetScreen.Y + (viewRect.Bottom - targetScreen.Y) / factor;

                var newLeft = (int)Math.Round(Math.Min(left, right));
                var newRight = (int)Math.Round(Math.Max(left, right));
                var newTop = (int)Math.Round(Math.Min(top, bottom));
                var newBottom = (int)Math.Round(Math.Max(top, bottom));

                if (newRight <= newLeft)
                    newRight = newLeft + 1;
                if (newBottom <= newTop)
                    newBottom = newTop + 1;

                _camera.WindowRect = System.Drawing.Rectangle.FromLTRB(newLeft, newTop, newRight, newBottom);
                camera = _camera;
                return true;
            }
            else
            {
                var camLoc = _camera.CameraLocation;
                var camDir = target - camLoc;
                if (!camDir.IsValid || camDir.IsZero)
                    return false;

                camDir.Unitize();
                _camera.viewportInfo.TargetPoint = target;
                _camera.viewportInfo.SetCameraDirection(camDir);
                _camera.viewportInfo.Camera35mmLensLength *= factor;
                camera = _camera;
                return true;
            }
        }
        /// <summary>
        /// Zoom:
        /// - Parallel: ViewRect factor scale <br/>
        /// - Perspective: LensLength factor scale <br/>
        /// Zoom changes the magnification of the view. In a perspective projection, this is achieved by changing the camera's lens length, which controls the field of view and thus the zoom level. In a parallel projection, since there is no perspective distortion, we simulate zooming by scaling the view rectangle, which effectively changes the size of the view while keeping the camera position fixed. The factor parameter controls how much to zoom, with values greater than 1 zooming in and values between 0 and 1 zooming out. A factor of 1 means no change in zoom.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static bool Zoom(ref CameraParameter camera, double factor)
        {
            CameraParameter _camera = DuplicateCameraParameter(camera, "Zoom");
            factor = factor <= 0 ? 1 : factor;
            if (_camera.IsParallel) //Zoom in by factor
            {

                var viewRect = camera.SourceType == CameraReference.RhinoReference ?
                CameraUtil.ViewRect(_camera.viewportInfo) : camera.WindowRect;
                var newWidth = (int)(viewRect.Width / factor);
                var newHeight = (int)(viewRect.Height / factor);
                var dx = (int)Math.Round((viewRect.Width - newWidth) / 2.0);
                var dy = (int)Math.Round((viewRect.Height - newHeight) / 2.0);

                viewRect.Inflate(-dx, -dy);
                _camera.WindowRect = viewRect;
                camera = _camera;
                return true;
            }
            else
            {
                _camera.viewportInfo.Camera35mmLensLength *= factor;
                camera = _camera;
                return true;
            }
        }
        public static bool Rotate(ref CameraParameter camera, double radians, Vector3d axis)
        {
            var _camera = DuplicateCameraParameter(camera, "Rotate");
            var rot = Transform.Rotation(radians, axis, _camera.CameraLocation);
            if(_camera.IsParallel)
            {
                var viewRect = CameraUtil.ViewRect(_camera.viewportInfo);
                var newCamDir = _camera.CameraDirection;
                newCamDir.Transform(rot);
                var newCamUp = _camera.CameraUp;
                newCamUp.Transform(rot);
                var newCamTar = _camera.CameraTarget;
                newCamTar.Transform(rot);

                _camera.viewportInfo.SetCameraUp(newCamUp);
                _camera.viewportInfo.SetCameraDirection(newCamDir);
                _camera.viewportInfo.TargetPoint = newCamTar;
                if(_camera.SourceType == CameraReference.RhinoReference)
                    _camera.WindowRect = viewRect;
                camera = _camera;
                return true;
            }
            else
            {                
                var newCamDir = _camera.CameraDirection;
                newCamDir.Transform(rot);
                var newCamUp = _camera.CameraUp;
                newCamUp.Transform(rot);
                var newCamTar = _camera.CameraTarget;
                newCamTar.Transform(rot);

                _camera.viewportInfo.SetCameraUp(newCamUp);
                _camera.viewportInfo.SetCameraDirection(newCamDir);
                _camera.viewportInfo.TargetPoint = newCamTar;
                camera = _camera;
                return true;
            }
                
            
        }
        public static bool Pan(ref CameraParameter camera, Vector3d panVector)
        {
            var _camera = DuplicateCameraParameter(camera, "Pan");
            if(!panVector.IsValid || panVector.IsZero)
            {
                return false;
            }
            if(_camera.IsParallel)
            {
                var viewRect = CameraUtil.ViewRect(_camera.viewportInfo);
                var camLoc = _camera.CameraLocation;
                var camTar = _camera.CameraTarget;
                camLoc += panVector;
                camTar += panVector;
                _camera.viewportInfo.SetCameraLocation(camLoc);
                _camera.viewportInfo.TargetPoint = camTar;
                if(_camera.SourceType == CameraReference.RhinoReference)
                    _camera.WindowRect = viewRect;
                camera = _camera;
                return true;
            }
            else
            {
                var camLoc = _camera.CameraLocation;
                var camTar = _camera.CameraTarget;
                camLoc += panVector;
                camTar += panVector;
                _camera.viewportInfo.SetCameraLocation(camLoc);
                _camera.viewportInfo.TargetPoint = camTar;
                camera = _camera;
                return true;
            }
        }
        public static bool Orbit(ref CameraParameter camera, double radians, Vector3d axis, Point3d center)
        {
            var _camera = DuplicateCameraParameter(camera, "Orbit");
            var rot = Transform.Rotation(radians, axis, center);
            if(_camera.IsParallel)
            {
                var viewRect = CameraUtil.ViewRect(_camera.viewportInfo);
                var newCamLoc = _camera.CameraLocation;
                newCamLoc.Transform(rot);
                var newCamDir = _camera.CameraDirection;
                var newCamUp = _camera.CameraUp;
                var newCamTar = _camera.CameraTarget;
                newCamUp.Transform(rot);
                newCamDir.Transform(rot);
                newCamTar.Transform(rot);
                _camera.viewportInfo.SetCameraLocation(newCamLoc);
                _camera.viewportInfo.SetCameraDirection(newCamDir);
                _camera.viewportInfo.SetCameraUp(newCamUp);
                _camera.viewportInfo.TargetPoint = newCamTar;
                if(_camera.SourceType == CameraReference.RhinoReference)
                    _camera.WindowRect = viewRect;
                camera = _camera;
                return true;
            }
            else
            {                
                var newCamLoc = _camera.CameraLocation;
                newCamLoc.Transform(rot);
                var newCamDir = _camera.CameraDirection;
                newCamDir.Transform(rot);
                var newCamUp = _camera.CameraUp;
                newCamUp.Transform(rot);
                var newCamTar = _camera.CameraTarget;
                newCamTar.Transform(rot);
                _camera.viewportInfo.SetCameraUp(newCamUp);
                _camera.viewportInfo.SetCameraLocation(newCamLoc);
                _camera.viewportInfo.SetCameraDirection(newCamDir);
                _camera.viewportInfo.TargetPoint = newCamTar;
                camera = _camera;
                return true;
            }
        }
        public static bool LookAt(ref CameraParameter camera, Point3d targetPoint, Vector3d upDirection, double lensLength)
        {
            
            var _camera = DuplicateCameraParameter(camera, "LookAt");
            if(!targetPoint.IsValid)
            {
                return false;
            }
            if(!upDirection.IsValid || upDirection.IsZero)
            {
                return false;
            }
            if(lensLength <= 0)
            {
                return false;
            }

            var camLoc = _camera.CameraLocation;
            var camDir = targetPoint - camLoc;
            if(!camDir.IsValid || camDir.IsZero)
            {
                return false;
            }
            camDir.Unitize();
            upDirection.Unitize();

             _camera.viewportInfo.SetCameraLocation(camLoc);
            _camera.viewportInfo.TargetPoint = targetPoint;
            _camera.viewportInfo.SetCameraDirection(camDir);
            _camera.viewportInfo.SetCameraUp(upDirection);
            _camera.viewportInfo.Camera35mmLensLength = lensLength;
            
            if(_camera.IsParallel)
            {
                var viewRect = CameraUtil.ViewRect(_camera.viewportInfo);
                if(_camera.SourceType == CameraReference.RhinoReference)
                    _camera.WindowRect = viewRect;
                camera = _camera;
                return true;
            }
            else
            {
                camera = _camera;
                return true;
            }
        }
    }
}
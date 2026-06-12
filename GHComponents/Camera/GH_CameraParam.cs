using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Woodpecker.Animation.Control.Camera;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    /// <summary>
    /// A Grasshopper parameter that stores a rhino named-view camera.
    /// </summary>
    public class GH_CameraParam : GH_PersistentParam<GH_CameraGoo>, IGH_PreviewObject
    {
        private CameraParameter _setupCamera;

        public GH_CameraParam() : base("Camera", "Camera", "A Grasshopper parameter that stores a rhino named-view camera", "Woodpecker", "Camera")
        {}
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override Bitmap Icon => Properties.Resources.GH_Cam_Param;
        
        public override Guid ComponentGuid => new Guid("7e7f4153-7008-4b0c-b450-4b99b158521c");

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            var result = base.AppendMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Create a Parallel Camera", Menu_CreateParallelCamera);
            Menu_AppendItem(menu, "Create a Perspective Camera", Menu_CreatePerspectiveCamera);
            return result;
        }
        private void Menu_CreateParallelCamera(object sender, EventArgs e)
        {
            CreateCamera(true);
        }
        private void Menu_CreatePerspectiveCamera(object sender, EventArgs e)
        {
            CreateCamera(false);
        }
        private void CreateCamera(bool parallel)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc?.Views.ActiveView == null)
                return;

            try
            {
                if (!TryGetPoint("Camera location", out var location))
                    return;

                if (!TryGetTarget(location, parallel, out var target))
                    return;

                if (!TryGetCameraUp(location, target, parallel, out var cameraUp))
                    return;

                var defaultValue = parallel
                    ? Math.Max(location.DistanceTo(target), 1.0)
                    : 50.0;

                _setupCamera = CreatePreviewCamera(
                    location,
                    target,
                    cameraUp,
                    parallel,
                    defaultValue);
                doc.Views.Redraw();

                var value = defaultValue;
                var prompt = parallel
                    ? "Parallel camera height"
                    : "Perspective lens length";

                var result = RhinoGet.GetNumber(prompt, false, ref value);
                if (result != Result.Success || value <= 0)
                    return;

                var camera = parallel
                    ? CameraParameter.CreateCameraParallel(location, target, cameraUp, value)
                    : CameraParameter.CreateCameraPerspective(location, target, cameraUp, value);

                RecordUndoEvent(parallel
                    ? "Create parallel camera"
                    : "Create perspective camera");
                SetPersistentData(new GH_CameraGoo(camera));
                ExpireSolution(true);
            }
            catch (Exception exception)
            {
                RhinoApp.WriteLine($"Camera creation failed: {exception.Message}");
            }
            finally
            {
                _setupCamera = null;
                doc.Views.Redraw();
            }
        }
        private static bool TryGetPoint(string prompt, out Point3d point)
        {
            using (var getter = new GetPoint())
            {
                getter.SetCommandPrompt(prompt);
                getter.Get();
                point = getter.Point();
                return getter.CommandResult() == Result.Success && point.IsValid;
            }
        }
        private bool TryGetTarget(
            Point3d location,
            bool parallel,
            out Point3d target)
        {
            using (var getter = new CameraPreviewGetter(
                this,
                location,
                Point3d.Unset,
                Vector3d.ZAxis,
                parallel,
                CameraSetupStage.Target))
            {
                getter.SetCommandPrompt("Camera target");
                getter.SetBasePoint(location, true);
                getter.DrawLineFromPoint(location, true);
                getter.Get();
                target = getter.Point();
                return getter.CommandResult() == Result.Success &&
                    target.IsValid &&
                    target.DistanceTo(location) > RhinoMath.ZeroTolerance;
            }
        }
        private bool TryGetCameraUp(
            Point3d location,
            Point3d target,
            bool parallel,
            out Vector3d cameraUp)
        {
            using (var getter = new CameraPreviewGetter(
                this,
                location,
                target,
                Vector3d.ZAxis,
                parallel,
                CameraSetupStage.Up))
            {
                getter.SetCommandPrompt("Camera up reference point");
                getter.SetBasePoint(target, true);
                getter.DrawLineFromPoint(target, true);
                getter.Get();

                cameraUp = getter.Point() - target;
                return getter.CommandResult() == Result.Success &&
                    cameraUp.IsValid &&
                    cameraUp.Unitize();
            }
        }
        private static CameraParameter CreatePreviewCamera(
            Point3d location,
            Point3d target,
            Vector3d cameraUp,
            bool parallel,
            double value)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc?.Views.ActiveView == null)
                return null;

            var direction = target - location;
            if (!direction.Unitize())
                return null;

            cameraUp -= direction * (cameraUp * direction);
            if (!cameraUp.Unitize())
            {
                cameraUp = Math.Abs(direction * Vector3d.ZAxis) < 0.99
                    ? Vector3d.ZAxis
                    : Vector3d.XAxis;
                cameraUp -= direction * (cameraUp * direction);
                cameraUp.Unitize();
            }

            var viewportInfo = new ViewportInfo(doc.Views.ActiveView.ActiveViewport);
            viewportInfo.SetCameraLocation(location);
            viewportInfo.TargetPoint = target;
            viewportInfo.SetCameraDirection(direction);
            viewportInfo.SetCameraUp(cameraUp);

            if (parallel)
            {
                viewportInfo.ChangeToParallelProjection(true);
                viewportInfo.GetFrustum(
                    out _,
                    out _,
                    out _,
                    out _,
                    out var near,
                    out var far);

                var aspect = doc.Views.ActiveView.ActiveViewport.Bounds.Height > 0
                    ? doc.Views.ActiveView.ActiveViewport.Bounds.Width /
                        (double)doc.Views.ActiveView.ActiveViewport.Bounds.Height
                    : 1.0;
                var halfHeight = Math.Max(value, 1e-6) * 0.5;
                var halfWidth = halfHeight * aspect;
                viewportInfo.SetFrustum(
                    -halfWidth,
                    halfWidth,
                    -halfHeight,
                    halfHeight,
                    near,
                    far);
            }
            else
            {
                var distance = location.DistanceTo(target);
                viewportInfo.ChangeToPerspectiveProjection(
                    distance,
                    true,
                    Math.Max(value, 1.0));
                viewportInfo.Camera35mmLensLength = Math.Max(value, 1.0);
            }

            return new CameraParameter("__Camera_Setup__", viewportInfo);
        }
        private static void DrawPreview(
            CameraParameter camera,
            DisplayPipeline display)
        {
            if (camera == null)
                return;

            foreach (var curve in CameraUtil.DisplayCamera(camera))
                display.DrawCurve(curve, Color.White, 2);

            var corners = CameraUtil.GetTargetRectCorners(camera);
            if (corners.Count == 4)
            {
                var frame = new Polyline(new[]
                {
                    corners[0],
                    corners[1],
                    corners[3],
                    corners[2],
                    corners[0]
                });
                display.DrawPolyline(frame, Color.YellowGreen, 2);
            }
        }
        private enum CameraSetupStage
        {
            Target,
            Up
        }
        private sealed class CameraPreviewGetter : GetPoint
        {
            private readonly GH_CameraParam _owner;
            private readonly Point3d _location;
            private readonly Point3d _target;
            private readonly Vector3d _cameraUp;
            private readonly bool _parallel;
            private readonly CameraSetupStage _stage;

            public CameraPreviewGetter(
                GH_CameraParam owner,
                Point3d location,
                Point3d target,
                Vector3d cameraUp,
                bool parallel,
                CameraSetupStage stage)
            {
                _owner = owner;
                _location = location;
                _target = target;
                _cameraUp = cameraUp;
                _parallel = parallel;
                _stage = stage;
            }

            protected override void OnDynamicDraw(GetPointDrawEventArgs e)
            {
                var target = _stage == CameraSetupStage.Target
                    ? e.CurrentPoint
                    : _target;
                var up = _stage == CameraSetupStage.Up
                    ? e.CurrentPoint - target
                    : _cameraUp;

                var defaultValue = _parallel
                    ? Math.Max(_location.DistanceTo(target), 1.0)
                    : 50.0;

                _owner._setupCamera = CreatePreviewCamera(
                    _location,
                    target,
                    up,
                    _parallel,
                    defaultValue);
                DrawPreview(_owner._setupCamera, e.Display);
                base.OnDynamicDraw(e);
            }
        }
        public bool Hidden {get;set;}
        public bool IsPreviewCapable => true;
        public BoundingBox ClippingBox
        {
            get
            {
                var hasbox = false;
                var box = BoundingBox.Empty;
                foreach(var goo in VolatileData.AllData(true))
                {
                    var cameraGoo = goo as GH_CameraGoo;
                    var camera = cameraGoo?.CameraValue;
                    if(camera == null) continue;

                    var crvs = CameraUtil.DisplayCamera(camera);
                    crvs.AddRange(CameraUtil.DisplayCameraDirection(camera.viewportInfo).Select(x => new LineCurve(x)));
                    foreach(var crv in crvs)
                    {
                        var crvBox = crv.GetBoundingBox(true);
                        if(!hasbox)
                        {
                            box = crvBox;
                            hasbox = true;
                        }
                        else
                        {
                            box.Union(crvBox);
                        }
                    }
                }
                if (_setupCamera != null)
                {
                    foreach (var curve in CameraUtil.DisplayCamera(_setupCamera))
                    {
                        var curveBox = curve.GetBoundingBox(true);
                        if (!hasbox)
                        {
                            box = curveBox;
                            hasbox = true;
                        }
                        else
                        {
                            box.Union(curveBox);
                        }
                    }
                }
                return hasbox ? box : BoundingBox.Empty;
            }
        }
        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            
        }
        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_setupCamera != null)
                _setupCamera.ShowCameraWire(this, args);

            foreach(var goo in VolatileData.AllData(true))
            {
                var cameraGoo = goo as GH_CameraGoo;
                if(cameraGoo?.CameraValue == null) continue;
                cameraGoo.CameraValue.ShowCameraWire(this, args);
            }
        }
        protected override GH_GetterResult Prompt_Plural(ref List<GH_CameraGoo> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref GH_CameraGoo value)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_CameraGoo InstantiateT()
        {
            return new GH_CameraGoo();
        }
    }
}

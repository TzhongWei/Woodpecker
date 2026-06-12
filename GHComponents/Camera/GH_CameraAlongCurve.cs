using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Moves a camera along a curve and optionally interpolates look-at and
    /// zoom values supplied by camera keyframes.
    /// </summary>
    public class GH_CameraAlongCurve : GH_CameraMotionAbstract
    {
        public override Guid ComponentGuid => new Guid("7faab6a6-8ff1-40e4-ba83-eff2f5999bc3");
        public GH_CameraAlongCurve() : base(
            "Camera Move On Curve",
            "CameraOnCrv",
            "Moves a camera along a curve and interpolates optional look-at and zoom keyframes.")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to move along the curve.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddCurveParameter("Curve", "Crv", "The path along which the camera location moves.", GH_ParamAccess.item);
            pManager.AddGenericParameter("KeyFrames", "Fs", "Optional camera keyframes defining normalized curve position, look-at target, and zoom factor.", GH_ParamAccess.list);
            pManager[3].Optional = true;
        }

        private bool _displayKeyFrames;
        private readonly List<CameraParameter> _keyFrameCameras = new List<CameraParameter>();

        private void DisplayKeyFrameCamera(object sender, EventArgs e)
        {
            _displayKeyFrames = !_displayKeyFrames;
            ExpirePreview(true);
            RhinoDoc.ActiveDoc?.Views.Redraw();
        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            base.AppendMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(
                menu,
                "Display Camera Frames",
                DisplayKeyFrameCamera,
                true,
                _displayKeyFrames);
            return true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the moving motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Reports whether this camera motion is active.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _keyFrameCameras.Clear();

            GH_CameraGoo cameraGoo = null;
            Curve curve = null;
            _t = -1.0;

            if (!DA.GetData("Camera Parameter", ref cameraGoo) ||
                cameraGoo?.CameraValue == null)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "A valid camera parameter is required.");
                SetInactiveOutputs(DA, cameraGoo);
                return;
            }

            if (!DA.GetData("Curve", ref curve) ||
                curve == null ||
                !curve.IsValid)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "A valid camera path curve is required.");
                SetInactiveOutputs(DA, cameraGoo);
                return;
            }

            DA.GetData("Pointer_t", ref _t);

            var keyFrames = ReadKeyFrames(DA);
            var motion = new CM_CamMoveAlongCrv(
                cameraGoo.CameraValue,
                curve,
                keyFrames);

            if (_displayKeyFrames)
                _keyFrameCameras.AddRange(motion.ShowAllKeyFrames());

            if (_t < 0)
            {
                SetInactiveOutputs(DA, cameraGoo);
                return;
            }

            _isActive = _applyCameraMotion;
            if (_isActive && HasMultipleActiveInstance())
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "More than one camera motion component is active.");
                DA.SetData("Camera Parameter", cameraGoo);
                DA.SetData("Status", false.ToString());
                return;
            }

            _cameraParam = cameraGoo.CameraValue;
            var execution = new CameraExecution(motion);
            if (!execution.Execute(_t, _applyCameraMotion))
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Warning,
                    "Pointer_t is outside the camera motion interval [0, 1].");
                SetInactiveOutputs(DA, cameraGoo);
                return;
            }

            _cameraParam = motion.MotionCamera;
            DA.SetData(
                "Camera Parameter",
                new GH_CameraGoo(_cameraParam));
            DA.SetData("Status", _isActive.ToString());
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                var box = BoundingBox.Empty;
                foreach (var camera in _keyFrameCameras)
                {
                    if (camera == null)
                        continue;

                    foreach (var curve in CameraUtil.DisplayCamera(camera))
                        box.Union(curve.GetBoundingBox(true));

                    foreach (var line in CameraUtil.DisplayCameraDirection(camera.viewportInfo))
                        box.Union(new LineCurve(line).GetBoundingBox(true));
                }

                return box;
            }
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (!_displayKeyFrames || Hidden)
                return;

            foreach (var camera in _keyFrameCameras)
                camera?.ShowCameraWire(this, args);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("DisplayKeyFrames", _displayKeyFrames);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean(
                "DisplayKeyFrames",
                ref _displayKeyFrames);
            return base.Read(reader);
        }

        private static List<KeyCameraFrames> ReadKeyFrames(
            IGH_DataAccess dataAccess)
        {
            var values = new List<object>();
            if (!dataAccess.GetDataList("KeyFrames", values))
                return new List<KeyCameraFrames>();

            return values
                .Select(UnwrapKeyFrame)
                .Where(frame => frame != null)
                .OrderBy(frame => frame.Nor_t)
                .ToList();
        }

        private static KeyCameraFrames UnwrapKeyFrame(object value)
        {
            if (value is KeyCameraFrames frame)
                return frame;

            if (value is GH_ObjectWrapper wrapper)
                return wrapper.Value as KeyCameraFrames;

            if (value is IGH_Goo goo)
                return goo.ScriptVariable() as KeyCameraFrames;

            return null;
        }

        private void SetInactiveOutputs(
            IGH_DataAccess dataAccess,
            GH_CameraGoo camera)
        {
            _isActive = false;
            dataAccess.SetData("Camera Parameter", camera);
            dataAccess.SetData("Status", _isActive.ToString());
        }
    }
}

using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_CameraZoom : GH_CameraMotionAbstract
    {
        public override Guid ComponentGuid => new Guid("99a2e6a1-f978-4c3d-b927-700e64e6c21f");
        public GH_CameraZoom() : base("Camera Zoom", "Zoom", "Create or evaluate a camera zoom motion from a key camera and zoom factor.") { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to use for the zoom motion.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddNumberParameter("Factor", "F", "Zoom in or out factor", GH_ParamAccess.item, 1);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the zoom motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);

        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double t = 0;
            double factor = 0;
            GH_CameraGoo cameraGoo = null;
            DA.GetData("Camera Parameter", ref cameraGoo);
            DA.GetData("Pointer_t", ref t);
            DA.GetData("Factor", ref factor);

            if (t < 0)
            {
                DA.SetData("Camera Parameter", cameraGoo);
                this._isActive = false;
                DA.SetData("Status", this._isActive.ToString());
                return;
            }
            this._isActive = this._applyCameraMotion;

            if (cameraGoo == null || cameraGoo.CameraValue == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid camera input.");
                return;
            }
            this._cameraParam = cameraGoo.CameraValue;
            var ZoomMotion = new CM_Zoom(this._cameraParam, factor);
            var executedCamera = new CameraExecution(ZoomMotion);
            executedCamera.Execute(t, this._applyCameraMotion);
            this._cameraParam = ZoomMotion.MotionCamera;

            DA.SetData("Camera Parameter", new GH_CameraGoo(this._cameraParam));
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}

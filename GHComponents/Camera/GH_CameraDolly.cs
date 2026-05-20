using System;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates a dolly camera motion based on a key camera and a distance to move the camera forward or backward. Inputs include Pointer_t and Distance. Outputs include Status.
    /// </summary>
    public class GH_CameraDolly : GH_CameraMotionAbstract
    {
        public GH_CameraDolly() : base("Camera Dolly", "Dolly", "Creates a dolly camera motion based on a key camera and a distance to move the camera forward or backward.") { }
        public override Guid ComponentGuid => new Guid("d1c9e5b8-8c3f-4a2b-9f1e-2b6a5c4d7e8f");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to use for the dolly motion.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddNumberParameter("Distance", "D", "The distance to move the camera forward (positive) or backward (negative).", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the dolly motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            double dist = 0.0;
            var t = 0.0;
            GH_CameraGoo cameraGoo = null;
            DA.GetData("Camera Parameter", ref cameraGoo);
            DA.GetData("Distance", ref dist);
            DA.GetData("Pointer_t", ref t);

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
            var dollyMotion = new CM_Dolly(this._cameraParam, dist);

            var executedCamera = new CameraExecution(dollyMotion);
            executedCamera.Execute(t, this._applyCameraMotion);
            this._cameraParam = dollyMotion.MotionCamera;

            DA.SetData("Camera Parameter", new GH_CameraGoo(this._cameraParam));
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}
using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Create or evaluate a camera motion that looks toward a target point. Inputs include Pointer_t, Target, Camera Up, and Camera Length. Outputs include Status.
    /// </summary>
    public class GH_CameraLookAt : GH_CameraMotionAbstract
    {
        public override Guid ComponentGuid => new Guid("6d8cc478-9b38-43df-9e8a-a80d1b3bcc7f");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Cam_LookAt;
        public GH_CameraLookAt() : base("Camera LookAt", "LookAt", "Create or evaluate a camera motion that looks toward a target point.") { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to use for the look at motion.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddPointParameter("Target", "Pt", "target point to look at", GH_ParamAccess.item);
            pManager.AddVectorParameter("Camera Up", "UpVec", "The camera up direction", GH_ParamAccess.item);
            pManager.AddNumberParameter("Camera Length", "Len", "The len of the camera", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the look at motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);

        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_CameraGoo cameraGoo = null;
            double t = 1.0;
            Point3d pt = new Point3d();
            Vector3d up = new Vector3d();
            double len = -1;

            DA.GetData("Camera Parameter", ref cameraGoo);
            DA.GetData("Pointer_t", ref t);
            DA.GetData("Target", ref pt);

            if (t < 0)
            {
                DA.SetData("Camera Parameter", cameraGoo);
                this._isActive = false;
                DA.SetData("Status", this._isActive.ToString());
                return;
            }
            if (cameraGoo == null || cameraGoo.CameraValue == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid camera input.");
                return;
            }
            if (!DA.GetData("Camera Up", ref up))
            {
                up = cameraGoo.CameraValue.CameraUp;
            }
            if (!DA.GetData("Camera Length", ref len))
            {
                len = cameraGoo.CameraValue.CameraLength;
            }
            this._cameraParam = cameraGoo.CameraValue;
            var LookAt = new CM_LookAt(this._cameraParam, pt, up, len);
            var executedCamera = new CameraExecution(LookAt);
            executedCamera.Execute(t, this._applyCameraMotion);
            this._cameraParam = LookAt.MotionCamera;

            DA.SetData("Camera Parameter", new GH_CameraGoo(this._cameraParam));
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}

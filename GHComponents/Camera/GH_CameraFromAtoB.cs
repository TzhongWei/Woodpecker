using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_CameraFromAtoB : GH_CameraMotionAbstract
    {
        public GH_CameraFromAtoB() : base("Camera from A to B", "CameraA2B", "Animating camera from camera A to camera B") { }
        public override Guid ComponentGuid => new Guid("a8f57800-907e-4775-976d-dc461f89efd3");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter A", "Cam A", "The camera to use for the camera start transition ", GH_ParamAccess.item);
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter B", "Cam B", "The Camera to use for the camera end transition", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the Cam A to Cam B transition motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double t = 0.0;
            DA.GetData("Pointer_t", ref t);
            GH_CameraGoo cam1 = null, cam2 = null;

            DA.GetData("Camera Parameter A", ref cam1);
            DA.GetData("Camera Parameter B", ref cam2);

            if (t < 0)
            {
                DA.SetData("Camera Parameter", cam1);
                this._isActive = false;
                DA.SetData("Status", this._isActive.ToString());
                return;
            }
            this._isActive = this._applyCameraMotion;

            if (cam1 == null || cam2 == null || cam1.CameraValue == null || cam2.CameraValue == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid camera input.");
                return;
            }

            this._cameraParam = cam1.CameraValue;
            var A2B = new CM_CamAToCamB(this._cameraParam, cam2.CameraValue);
            var executeCamera = new CameraExecution(A2B);
            executeCamera.Execute(t, this._applyCameraMotion);
            this._cameraParam = A2B.MotionCamera;
            DA.SetData("Camera Parameter", this._cameraParam);
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}
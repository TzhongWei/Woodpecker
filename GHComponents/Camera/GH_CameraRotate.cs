using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_CameraRotate : GH_CameraMotionAbstract
    {
        public override Guid ComponentGuid => new Guid("3439ce50-7d12-4992-9c4f-14c820c8a934");
        public GH_CameraRotate() : base("Camera Rotation", "Rotate", "Rotate a camera") { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to use for the rotate motion.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddAngleParameter("Angle", "A", "camera rotation angle", GH_ParamAccess.item);
            pManager.AddVectorParameter("Axis", "X", "Rotation axis", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the rotate motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);

        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_CameraGoo cameraGoo = null;
            double t = 0;
            double angle = 0;
            Vector3d axis = new Vector3d();
            DA.GetData("Camera Parameter", ref cameraGoo);
            DA.GetData("Pointer_t", ref t);
            DA.GetData("Angle", ref angle);
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
            if (!DA.GetData("Axis", ref axis))
            {
                axis = cameraGoo.CameraValue.CameraUp;
            }
            this._cameraParam = cameraGoo.CameraValue;
            var rotateMotion = new CM_Rotate(_cameraParam, angle, axis);
            var executedCamera = new CameraExecution(rotateMotion);
            executedCamera.Execute(t, this._applyCameraMotion);
            this._cameraParam = rotateMotion.MotionCamera;

            DA.SetData("Camera Parameter", new GH_CameraGoo(this._cameraParam));
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}
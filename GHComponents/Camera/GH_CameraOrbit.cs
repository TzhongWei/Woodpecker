using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Create or evaluate an orbit camera motion around a centre and axis. Inputs include Pointer_t, Angle, Axis, and Centre. Outputs include Status.
    /// </summary>
    public class GH_CameraOrbit : GH_CameraMotionAbstract
    {
        public override Guid ComponentGuid => new Guid("4649731d-e967-41b7-9caf-3dac592b2d03");
        public GH_CameraOrbit() : base("Camera Orbit", "Orbit", "Create or evaluate an orbit camera motion around a centre and axis.") { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to use for the orbit motion.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddAngleParameter("Angle", "A", "Orbit rotation angle", GH_ParamAccess.item);
            pManager.AddVectorParameter("Axis", "X", "The axis of the rotation", GH_ParamAccess.item);
            pManager.AddPointParameter("Centre", "C", "Rotate centre", GH_ParamAccess.item);
            pManager[4].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the orbit motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);

        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double angle = 1.0;
            Vector3d axis = new Vector3d();
            double t = 1.0;

            GH_CameraGoo cameraGoo = null;
            DA.GetData("Camera Parameter", ref cameraGoo);
            DA.GetData("Angle", ref angle);
            DA.GetData("Axis", ref axis);
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
            var centre = cameraGoo.CameraValue.CameraTarget;
            var _tempPt = new Point3d();
            if (DA.GetData("Centre", ref _tempPt))
            {
                centre = _tempPt;
            }
            this._cameraParam = cameraGoo.CameraValue;

            var orbitMotion = new CM_Orbit(this._cameraParam, angle, axis, centre);
            var executedCamera = new CameraExecution(orbitMotion);
            executedCamera.Execute(t, this._applyCameraMotion);
            this._cameraParam = orbitMotion.MotionCamera;

            DA.SetData("Camera Parameter", new GH_CameraGoo(this._cameraParam));
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}

using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Create or evaluate a camera pan motion using a world-space translation vector. Inputs include Pointer_t and Pan Vector. Outputs include Status.
    /// </summary>
    public class GH_CameraPan : GH_CameraMotionAbstract
    {
        public GH_CameraPan() : base("Camera Pan", "Pan", "Create or evaluate a camera pan motion using a world-space translation vector.") { }
        public override Guid ComponentGuid => new Guid("ae84f980-6ae6-4d98-8736-aa869b68f32a");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Cam_Pan;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The camera to use for the pan motion.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "The time parameter to evaluate the camera motion, where 0 is the start and 1 is the end of the motion. If t < 0, the motion is not activated.", GH_ParamAccess.item, -1.0);
            pManager.AddVectorParameter("Pan Vector", "Vec", "World-space vector used to pan the camera position and target.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera Parameter", "Cam", "The resulting camera after applying the pan motion.", GH_ParamAccess.item);
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);

        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_CameraGoo cameraGoo = null;
            _t = 0;
            Vector3d pan = new Vector3d(0, 0, 0);
            DA.GetData("Pointer_t", ref _t);
            DA.GetData("Pan Vector", ref pan);
            DA.GetData("Camera Parameter", ref cameraGoo);

            if (_t < 0)
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
            var panMotion = new CM_Pan(this._cameraParam, pan);
            var executedCamera = new CameraExecution(panMotion);
            executedCamera.Execute(_t, this._applyCameraMotion);
            this._cameraParam = panMotion.MotionCamera;
            DA.SetData("Camera Parameter", new GH_CameraGoo(this._cameraParam));
            DA.SetData("Status", this._isActive.ToString());
        }
    }
}

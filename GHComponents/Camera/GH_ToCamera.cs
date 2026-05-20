using System;
using Eto.Forms;
using Grasshopper.Kernel;
using Rhino;
using Rhino.PlugIns;
using Woodpecker.Animation.GHComponents.CustomGHComponents;


namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Adjust current viewpoint to the given camera. Inputs include Run. Outputs include Status.
    /// </summary>
    public class GH_ToCamera : GH_CameraMotionAbstract
    {
        public GH_ToCamera() : base("To Camera", "TCam", "Adjust current viewpoint to the given camera") { }
        public override Guid ComponentGuid => new Guid("03ddb948-d84e-4072-8c17-8eeab11dbbd5");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera", "Cam", "A camera you want to adapt to current viewport", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Fit the camera into current viewport", GH_ParamAccess.item, false);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Act", "Output if the this component is activated", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this._cameraParam = null;
            GH_CameraGoo cameraGoo = null;
            DA.GetData("Camera", ref cameraGoo);

            if (cameraGoo == null || cameraGoo.CameraValue == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid camera input.");
                return;
            }
            this._cameraParam = cameraGoo.CameraValue;
            bool activate = false;
            DA.GetData("Run", ref activate);

            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
                throw new Exception("Rhino Doc is null");

            var vp = doc.Views.ActiveView;
            var avp = vp.ActiveViewport;

            if (activate && this._applyCameraMotion)
            {
                this._isActive = true;
                doc.NamedViews.Restore(doc.NamedViews.FindByName(_cameraParam.Name), avp);
            }
            else
            {
                this._isActive = false;
            }

            DA.SetData("Status", _isActive.ToString());
        }
    }
}
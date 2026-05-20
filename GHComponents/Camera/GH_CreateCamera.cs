using Grasshopper.Kernel;
using System;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Rhino;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Create a camera parameter from a rhino named view or a phantom camera info. Inputs include Camera Name. Outputs include Camera Parameter.
    /// </summary>
    public class GH_CreateCamera : GH_Component
    {
        public GH_CreateCamera() : base("Create Camera", "Create Camera", "Create a camera parameter from a rhino named view or a phantom camera info", "Woodpecker", "Camera")
        { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("d1c8b9e7-5a3c-4f0e-9b2c-8a1f0e5b6c7d");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Camera Name", "Name", "The name of the rhino named view to reference. If the name is not found, it will try to create a phantom camera with this name.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Camera Parameter", "Cam", "The camera parameter created from the input.", GH_ParamAccess.item);
        }
        public void CreateCamera()
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "RhinoDoc.ActiveDoc cannot be null");
                return;
            }
            if (string.IsNullOrEmpty(_newCameraName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Camera name cannot be null or empty");
                return;
            }
            var currentview = doc.Views.ActiveView;
            if (currentview == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active view found in Rhino");
                return;
            }
            _cameraParameter = CameraParameter.CreateCameraParameter(_newCameraName, currentview.ActiveViewport, true);
            this.Message = _cameraParameter.Name;
            this.ExpireSolution(true);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Create", CreateCamera, "From current camera View");
        }
        private string _newCameraName = null;
        private CameraParameter _cameraParameter = null;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Camera Name", ref _newCameraName)) return;
            if(_cameraParameter == null)
                return;
            DA.SetData("Camera Parameter", new GH_CameraGoo(_cameraParameter));
        }
    }
}

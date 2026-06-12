using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_CameraKeyFramesSetting : GH_Component
    {
        public GH_CameraKeyFramesSetting():base("Camera Key Frames setting", "CamFrameS", "", "Woodpecker", "Camera"){}
        public override Guid ComponentGuid => new Guid("117f249e-f162-49f1-8a1e-f016a76579ed");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Curve t", "Crv_t", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Look At", "Target", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Camera Up", "Up", "", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddNumberParameter("Zoom Factor", "ZF", "", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Key Camera", "Fs", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double crv_t = 0.0;
            Point3d lookat = Point3d.Unset;
            double factor = 0.0;
            Vector3d up = Vector3d.Unset;
            DA.GetData("Curve t", ref crv_t);
            DA.GetData("Look At", ref lookat);
            DA.GetData("Zoom Factor", ref factor);
            DA.GetData("Camera Up", ref up);
            crv_t = Math.Min(1, Math.Max(0, crv_t));

            DA.SetData("Key Camera", new KeyCameraFrames(crv_t, lookat, factor, up));
        }
    }
}
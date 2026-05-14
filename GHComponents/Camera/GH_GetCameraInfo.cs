using Grasshopper.Kernel;
using System;
using System.Linq;
using Woodpecker.Animation.Control.Camera;


namespace Woodpecker.Animation.GHComponents
{
    public class GH_GetCameraInfo : GH_Component
    {
        public GH_GetCameraInfo() : base("Get Camera Info", "GetCamInfo", "Get the camera information of a rhino named view", "Woodpecker", "Camera")
        { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("d1c8b9e7-5a3c-4f0c-9a1e-2b8f8c8c8c8c");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new CustomGHComponents.GH_CameraParam(), "Camera Parameter", "Cam", "Camera parameter from a rhino named view", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Is Parallel Projection", "IsParallel", "Whether the camera is using parallel projection or perspective projection", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Camera Plane", "CamPlane", "The plane representing the camera location and orientation", GH_ParamAccess.item);
            pManager.AddVectorParameter("Camera Direction", "CamDir", "The direction vector of the camera", GH_ParamAccess.item);
            pManager.AddVectorParameter("Camera Up", "CamUp", "The up vector of the camera", GH_ParamAccess.item);
            pManager.AddPointParameter("Camera Target", "CamTarget", "The target point that the camera is looking at", GH_ParamAccess.item);
            pManager.AddPointParameter("Camera Location", "CamLoc", "The location point of the camera in 3D space", GH_ParamAccess.item);
            pManager.AddNumberParameter("Camera Length (mm)", "CamLen", "The 35mm lens length of the camera, only valid when in perspective projection mode. A smaller value means a wider field of view.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Window Border", "WinBorder", "The curve representing the camera window border in the Rhino viewport, useful for aligning geometry to the camera view. Note that this curve is in world coordinates, not screen coordinates.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cameraGoo = new CustomGHComponents.GH_CameraGoo();
            if (!DA.GetData(0, ref cameraGoo) || cameraGoo.CameraValue == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Camera Parameter");
                return;
            }
            var camParam = cameraGoo;
            DA.SetData(0, camParam.CameraValue.IsParallel);
            DA.SetData(1, camParam.Value);
            DA.SetData(2, camParam.CameraValue.CameraDirection);
            DA.SetData(3, camParam.CameraValue.CameraUp);
            DA.SetData(4, camParam.CameraValue.CameraTarget);
            DA.SetData(5, camParam.CameraValue.CameraLocation);
            DA.SetData(6, camParam.CameraValue.CameraLength);
            var cameraRectPts = CameraUtil.GetTargetRectCorners(camParam.CameraValue);
            if (cameraRectPts != null)
            {
                {
                    var indices = new int[] { 0, 1, 3, 2, 0 };
                    var rectCrv = new Rhino.Geometry.PolylineCurve(indices.Select(i => cameraRectPts[i]));
                    DA.SetData(7, rectCrv);
                }
            }
        }
    }
}

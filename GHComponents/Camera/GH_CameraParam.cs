using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Camera;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    /// <summary>
    /// A Grasshopper parameter that stores a rhino named-view camera.
    /// </summary>
    public class GH_CameraParam : GH_PersistentParam<GH_CameraGoo>, IGH_PreviewObject
    {
        public GH_CameraParam() : base("Camera", "Camera", "A Grasshopper parameter that stores a rhino named-view camera", "Woodpecker", "Camera")
        {}
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("7e7f4153-7008-4b0c-b450-4b99b158521c");

        
        public bool Hidden {get;set;}
        public bool IsPreviewCapable => true;
        public BoundingBox ClippingBox
        {
            get
            {
                var hasbox = false;
                var box = BoundingBox.Empty;
                foreach(var goo in VolatileData.AllData(true))
                {
                    var cameraGoo = goo as GH_CameraGoo;
                    var camera = cameraGoo?.CameraValue;
                    if(camera == null) continue;

                    var crvs = CameraUtil.DisplayCamera(camera);
                    crvs.AddRange(CameraUtil.DisplayCameraDirection(camera.viewportInfo).Select(x => new LineCurve(x)));
                    foreach(var crv in crvs)
                    {
                        var crvBox = crv.GetBoundingBox(true);
                        if(!hasbox)
                        {
                            box = crvBox;
                            hasbox = true;
                        }
                        else
                        {
                            box.Union(crvBox);
                        }
                    }
                }
                return hasbox ? box : BoundingBox.Empty;
            }
        }
        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            
        }
        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            foreach(var goo in VolatileData.AllData(true))
            {
                var cameraGoo = goo as GH_CameraGoo;
                if(cameraGoo?.CameraValue == null) continue;
                cameraGoo.CameraValue.ShowCameraWire(this, args);
            }
        }
        protected override GH_GetterResult Prompt_Plural(ref List<GH_CameraGoo> values)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_GetterResult Prompt_Singular(ref GH_CameraGoo value)
        {
            return GH_GetterResult.cancel;
        }
        protected override GH_CameraGoo InstantiateT()
        {
            return new GH_CameraGoo();
        }
    }
}

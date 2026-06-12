using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.Geometry.Display;
using Rhino.Geometry;
using System.Drawing;
using Woodpecker.Animation.Util.IO;
using Grasshopper;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayClippingGeometry : GH_DisplayGeometryAbstract
    {
        public GH_DisplayClippingGeometry():
        base("Display Clipping Geometry", 
        "DispCGeo", "")
        {
            _renderClippingPipeline = new RenderClippingPipeline(
                new List<DisplayClippingContent>(),
                this,
                GeometryRenderMode.Shaded
            );
            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_renderClippingPipeline);
            _conduit.Enabled = true;
        }

        public override Guid ComponentGuid => new Guid("8e849219-973e-4aac-bebd-d8a477e89e67");

        protected override IRenderPipeline renderPipeline => _renderClippingPipeline;
        private readonly RenderClippingPipeline _renderClippingPipeline;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to Clip", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("Clipping Planes", "CLs", "the planes to clip the geometry", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour", "Col", "Color of the geometry.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Show Clipping Section", "S", "", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            
        }
        public override BoundingBox ClippingBox => _renderClippingPipeline?.ClippingBox ?? BoundingBox.Empty;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cliplist = new List<Plane>();
            if(!DA.GetDataTree<IGH_GeometricGoo>("Geometry", out var geometryTree) || geometryTree == null)
            {
                ClearDisplayContents();
                return;
            }
            if(!DA.GetDataList("Clipping Planes", cliplist) || cliplist.Count == 0 || cliplist == null)
            {
                ClearDisplayContents();
                return;
            }
            var colour = Color.Black;
            DA.GetData("Colour", ref colour);
            var showSect = true;
            DA.GetData("Show Clipping Section", ref showSect);
            DA.GetDataTree<GH_Number>("Pointer_t", out var tTree);
            DataTree<GeometryBase> geomTree = new DataTree<GeometryBase>();
            DataTree<double> _tTree = new DataTree<double>();
            DataUtil.GH_Structure2GH_DataTree(geometryTree, ref geomTree);
            DataUtil.GH_Structure2GH_DataTree(tTree, ref _tTree);
            _tTree = DataUtil.AlignDataTree(geomTree, _tTree);
            var geomList = geomTree.AllData();
            var tList = _tTree.AllData();
            var contents = new List<DisplayClippingContent>();
            for(int i = 0; i < geomList.Count; i++)
            {
                var content = new DisplayClippingContent(
                    geomList[i], colour, cliplist, showSect
                );
                content.SetT(tList[i]);
                contents.Add(content);
            }

            _renderClippingPipeline.Stage = SelectedRenderStage;
            _renderClippingPipeline.SetContents(contents);
            SynchronizePreviewState();
        }
    }
}
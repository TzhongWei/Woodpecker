using System;
using System.Collections.Generic;
using System.Data.Common;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayVector : GH_DisplayGeometryAbstract
    {
        public override Guid ComponentGuid => new Guid("c9aca4e1-933b-49d3-94ee-ee4e2245f02f");
        public GH_DisplayVector():base("Vector Display", "VD", "Legacy viewport vector-arrow display component.")
        {
            _renderVectorPipeline = new RenderVectorPipeline(
                new List<DisplayVectorContent>(),
                this,
                VectorRenderMode.Linear
            );
            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_renderVectorPipeline);
            _conduit.Enabled = true;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Vec;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Arrow Anchor", "AP", "Arrow Anchors.", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Arrow Direction", "AV", "Arrow direction vectors.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Vector Display Setting", "VDs", "Optional vector display style settings.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
            pManager[2].Optional = true;
        }
        protected override IRenderPipeline renderPipeline => _renderVectorPipeline;
        private readonly RenderVectorPipeline _renderVectorPipeline;
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree<GH_Vector>("Arrow Direction", out var vecTree) ||
                vecTree == null ||
                vecTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }
            if (!DA.GetDataTree<GH_Point>("Arrow Anchor", out var ptTree) ||
                ptTree == null ||
                ptTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }
            var vecSetting = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};
            DA.GetData("Vector Display Setting", ref vecSetting);
            if (!DA.GetDataTree<GH_Number>("Pointer_t", out var tTree) ||
                tTree == null ||
                tTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }

            var _arrowDirection = new DataTree<Vector3d>();
            var _arrowTarget = new DataTree<Point3d>();
            var _pointer_ts = new DataTree<double>();

            DataUtil.GH_Structure2GH_DataTree(ptTree, ref _arrowTarget);
            DataUtil.GH_Structure2GH_DataTree(vecTree, ref _arrowDirection);
            DataUtil.GH_Structure2GH_DataTree(tTree, ref _pointer_ts);
            var contents = new List<DisplayVectorContent>();
            _arrowTarget = DataUtil.AlignDataTree(_arrowDirection, _arrowTarget);
            _pointer_ts = DataUtil.AlignDataTree(_arrowDirection, _pointer_ts);

            var directions = _arrowDirection.AllData();
            var anchors = _arrowTarget.AllData();
            var times = _pointer_ts.AllData();

            for(int i = 0; i < directions.Count; i++)
            {
                if (!directions[i].IsValid || !anchors[i].IsValid)
                    continue;

                var content = new DisplayVectorContent(anchors[i], directions[i], vecSetting);
                content.SetT(times[i]);
                contents.Add(content);
            }
            _renderVectorPipeline.Stage = SelectedRenderStage;
            _renderVectorPipeline.SetContents(contents);
            SynchronizePreviewState();
        }
        public override BoundingBox ClippingBox => _renderVectorPipeline?.ClippingBox ?? BoundingBox.Empty;
    }
}

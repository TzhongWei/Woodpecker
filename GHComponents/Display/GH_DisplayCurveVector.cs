using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Geometry.Processing;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayCurveVector : GH_DisplayGeometryAbstract
    {
        public GH_DisplayCurveVector() : base("Vector Curve Display", "VCD", "Display vector on a curve with a curved arrow")
        {
            _renderVectorPipeline = new RenderVectorPipeline(
                new List<DisplayVectorContent>(),
                this,
                VectorRenderMode.Curved
            );
            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_renderVectorPipeline);
            _conduit.Enabled = true;
        }
        public override Guid ComponentGuid => new Guid("0d2b411d-52d6-439a-a957-103088499035");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Vec_Crv;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curves on which curved arrows are displayed.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Curve t", "Ct", "a normalised value used to place the vector head. Ct should be [0,1]", GH_ParamAccess.tree);
            pManager.AddGenericParameter("VectorDisplaySetting", "VDs", "Optional vector display style settings.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
            pManager[2].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

        }
        protected override IRenderPipeline renderPipeline => _renderVectorPipeline;
        private readonly RenderVectorPipeline _renderVectorPipeline;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree<GH_Curve>("Curve", out var cTree) ||
                cTree == null ||
                cTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }
            if (!DA.GetDataTree<GH_Number>("Curve t", out var ctTree) ||
                ctTree == null ||
                ctTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }
            if (!DA.GetDataTree<GH_Number>("Pointer_t", out var tTree) ||
                tTree == null ||
                tTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }
            var vecDisSetting = new VectorDisplaySetting() { Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0 };
            DA.GetData("VectorDisplaySetting", ref vecDisSetting);
            var _curveTree = new DataTree<Curve>();
            var _ctTree = new DataTree<double>();
            var _tTree = new DataTree<double>();
            DataUtil.GH_Structure2GH_DataTree(cTree, ref _curveTree);
            DataUtil.GH_Structure2GH_DataTree(ctTree, ref _ctTree);
            DataUtil.GH_Structure2GH_DataTree(tTree, ref _tTree);
            _ctTree = DataUtil.AlignDataTree(_curveTree, _ctTree);
            _tTree = DataUtil.AlignDataTree(_curveTree, _tTree);
            var crvList = _curveTree.AllData();
            var ctList = _ctTree.AllData();
            var tList = _tTree.AllData();
            var contents = new List<DisplayVectorContent>();
            for (int i = 0; i < crvList.Count; i++)
            {
                if (crvList[i] == null || !crvList[i].IsValid)
                    continue;

                PathUtil.DrawPath(crvList[i], ctList[i], Vector3d.ZAxis, 0, out _, out var finishPath);

                if (finishPath == null || !finishPath.IsValid)
                    continue;

                var content = new DisplayVectorContent(finishPath, vecDisSetting);
                content.SetT(tList[i]);
                contents.Add(content);
            }
            _renderVectorPipeline.Stage = SelectedRenderStage;
            _renderVectorPipeline.SetContents(contents);
            SynchronizePreviewState();
        }
    }
}

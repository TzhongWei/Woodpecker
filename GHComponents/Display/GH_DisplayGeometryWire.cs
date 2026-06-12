using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayGeometryWire : GH_DisplayGeometryAbstract
    {
        public GH_DisplayGeometryWire() :
        base("GeometryWire Display",
            "WireDisplay",
            "Display geometry as wires in the viewport with custom color and width. Supports edge display for curves and extracted geometry edges, and can optionally draw view-dependent silhouettes. Unlike DisplayGeometry, this component only draws wireframe/outline information and does not render shaded surfaces.")
        {
            _renderGeometryPipeline = new RenderGeometryPipeline(
                new List<DisplayGeometryContent>(),
                this,
                GeometryRenderMode.Wire
            );
            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_renderGeometryPipeline);
            _conduit.Enabled = true;
        }
        public override Guid ComponentGuid => new Guid("4ccbb571-3708-4eca-ab65-03996e580e76");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to display.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Dash Pattern", "Pattern", "Dash pattern setting used for wire display.", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "Col", "Color of the geometry.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Line width for wireframe, edges, and silhouette display.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
            pManager[1].Optional = true;
        }
        private bool _silhouette = true;
        private bool _widthChangeByView = false;
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Draw Outline", ActivativeSilhouette, true, _silhouette);
            Menu_AppendItem(menu, "Width Changed from view", WidthChangedByView, true, _widthChangeByView);
        }
        private void WidthChangedByView(object sender, EventArgs e)
        {
            _widthChangeByView = !_widthChangeByView;
        }
        private void ActivativeSilhouette(object sender, EventArgs e)
        {
            _silhouette = !_silhouette;
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Silhouette", _silhouette);
            writer.SetBoolean("WidthChangeByView", _widthChangeByView);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("Silhouette", ref _silhouette);
            reader.TryGetBoolean("WidthChangeByView", ref _widthChangeByView);
            return base.Read(reader);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No outputs needed for display component
        }
        protected override IRenderPipeline renderPipeline => _renderGeometryPipeline;
        private readonly RenderGeometryPipeline _renderGeometryPipeline;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Wire;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree<IGH_GeometricGoo>("Geometry", out var geometryTree) || geometryTree == null)
            {
                ClearDisplayContents();
                return;
            }
            DashType dashType = DashType.Continuous;
            DA.GetData("Dash Pattern", ref dashType);

            var colour = Color.Black;
            DA.GetData("Color", ref colour);
            int width = 1;
            DA.GetData("Width", ref width);
            DA.GetDataTree<GH_Number>("Pointer_t", out var t_Tree);

            var geoTree = new DataTree<GeometryBase>();
            DataUtil.GH_Structure2GH_DataTree(geometryTree, ref geoTree);
            var tTree = new DataTree<double>();
            DataUtil.GH_Structure2GH_DataTree(t_Tree, ref tTree);

            tTree = DataUtil.AlignDataTree(geoTree, tTree);

            var geoList = geoTree.AllData();
            var tList = tTree.AllData();
            var contents = new List<DisplayGeometryContent>();
            for (int i = 0; i < geoList.Count; i++)
            {
                var content = new DisplayGeometryContent(geoList[i], colour);
                content.SetT(tList[i]);
                content.DoSilhouette = _silhouette;
                content.Width = width;
                content.dashType = dashType;
                content.WidthChangeFromView = _widthChangeByView;
                contents.Add(content);
            }
            _renderGeometryPipeline.Stage = SelectedRenderStage;
            _renderGeometryPipeline.SetContents(contents);
            SynchronizePreviewState();
        }
        public override BoundingBox ClippingBox => _renderGeometryPipeline?.ClippingBox ?? BoundingBox.Empty;
    }
}

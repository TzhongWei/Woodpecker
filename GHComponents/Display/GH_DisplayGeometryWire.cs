using Grasshopper.Kernel;
using Grasshopper;
using System;
using Rhino.Geometry;
using System.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Geometry;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Woodpecker.Animation.Geometry.Processing;
using GH_IO.Serialization;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayGeometryWire : GH_Component
    {
        public GH_DisplayGeometryWire() : base(
            "GeometryWire Display",
            "WireDisplay",
            "Display geometry as wires in the viewport with custom color and width. Supports edge display for curves and extracted geometry edges, and can optionally draw view-dependent silhouettes. Unlike DisplayGeometry, this component only draws wireframe/outline information and does not render shaded surfaces.",
            "Woodpecker",
            "Display")
        {

        }
        public override bool Write(GH_IWriter writer)
        {

            return base.Write(writer);
        }
        private bool _flatShading = true;
        private bool _widthChangeByView = false;
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Draw Outline", ActivativeFlatShading, true, _flatShading);
            Menu_AppendItem(menu, "Width Changed from view", WidthChangedByView, true, _widthChangeByView);
        }
        private void WidthChangedByView(object sender, EventArgs e)
        {
            _widthChangeByView = !_widthChangeByView;
        }
        private void ActivativeFlatShading(object sender, EventArgs e)
        {
            _flatShading = !_flatShading;
        }
        public override Guid ComponentGuid => new Guid("b01309c8-0916-46db-a51c-548e62453c8b");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to display.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Dash Pattern", "Pattern", "", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "Col", "Color of the geometry.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Line width for wireframe, edges, and silhouette display.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No outputs needed for display component
        }
        private DisplayGeometry _displayGeo;
        protected override void BeforeSolveInstance()
        {
            _displayGeo = null; // Reset display geometry before each solution
        }
        DashType _dashType;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DashType dashType = DashType.Continuous;
            DA.GetData("Dash Pattern", ref dashType);
            
            this._dashType = dashType;
            

            if(!DA.GetDataTree<IGH_GeometricGoo>(0, out var iGs))
            {
                return;
            }
            Color col = Color.Black;

            DA.GetData("Color", ref col);
            int width = 1;
            DA.GetData("Width", ref width);
            DA.GetDataTree<GH_Number>(4, out var t);

            var geoTree = new DataTree<GeometryBase>();
            DataUtil.GH_Structure2GH_DataTree(iGs, ref geoTree);
            // for (int i = 0; i < iGs.Branches.Count; i++)
            // {
            //     geoTree.AddRange(iGs.Branches[i].Select(x => x.ScriptVariable() as GeometryBase), new GH_Path(i));
            // }
            var tTree = new DataTree<double>();
            DataUtil.GH_Structure2GH_DataTree(t, ref tTree);
            // for (int i = 0; i < t.Branches.Count; i++)
            // {
            //     tTree.AddRange(t.Branches[i].ConvertAll(n => n.Value), new GH_Path(i));
            // }
            _iGs = geoTree;


            _displayGeo = new DisplayGeometry(geoTree, col, width, tTree);
            
        }
        public override BoundingBox ClippingBox => _displayGeo?.ClippingBox ?? BoundingBox.Empty;
        private DataTree<Color> _iColors => this._displayGeo?.GetColors() ?? new DataTree<Color>();
        private DataTree<double> _transparency => this._displayGeo?.GetTransparency() ?? new DataTree<double>();
        private DataTree<GeometryBase> _iGs;
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // for (int i = 0; i < _iColors.BranchCount; i++)
            // {
            //     for (int j = 0; j < _iColors.Branch(i).Count; j++)
            //     {
            //         var LocTransparency = _transparency.Branch(i)[j];
            //         var TempCol = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : _iColors.Branch(i)[j];
            //         var DisplayMaterial = _flatShading ? new Rhino.Display.DisplayMaterial(TempCol, TempCol, TempCol, TempCol, 0, LocTransparency) :
            //         new Rhino.Display.DisplayMaterial(TempCol, LocTransparency);
            //         if (LocTransparency >= 0.99) continue; // Skip fully transparent geometry
            //         var Geom = _iGs.Branch(i)[j];
            //         if (Geom is Mesh iM)
            //         {
            //             args.Display.DrawMeshShaded(iM, DisplayMaterial);
            //         }
            //         else if (Geom is Brep iB)
            //             foreach (var M in Mesh.CreateFromBrep(iB, MeshingParameters.Default))
            //             {
            //                 args.Display.DrawMeshShaded(M, DisplayMaterial);
            //             }
            //     }
            // }
        }
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            Color TempCol = new Color();
            for (int i = 0; i < _iColors.BranchCount; i++)
            {
                var LocTransparency = _transparency.Branch(i).First();
                if (LocTransparency >= 0.99) continue; // Skip fully transparent geometry
                TempCol = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : _iColors.Branch(i).First();
                var Geom = _displayGeo.GetEdge().Branch(i);
                foreach (var crv in Geom)
                {
                    var curveDisplay = new CurveDisplay(crv, _dashType);
                    foreach (var seg in curveDisplay.GetCurvesByDashType())
                        args.Display.DrawCurve(seg, TempCol, _displayGeo.GetWidth(_widthChangeByView));
                }

                if (_flatShading)
                {
                    foreach (var crv in _displayGeo.GetGeoms().Branch(i).SelectMany(x => DisplayUtil.DisplaySilhouette(x)))
                    {
                        if(crv == null) continue;
                        var curveDisplay = new CurveDisplay(crv, _dashType);
                        foreach (var seg in curveDisplay.GetCurvesByDashType())
                            args.Display.DrawCurve(seg, TempCol, _displayGeo.GetWidth(_widthChangeByView));
                    }
                }
            }
        }

    }
}

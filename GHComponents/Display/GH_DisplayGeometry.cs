using System.Collections.Generic;
using Grasshopper;
using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;
using Grasshopper.Kernel.Geometry.SpatialTrees;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Displays geometry in the viewport using colour and transparency settings. Geometry, colour, and display options are read as Grasshopper data and drawn as preview geometry. Inputs include Geometry, Color, and Pointer_t.
    /// </summary>
    public class GH_DisplayGeometry : GH_Component
    {
        public GH_DisplayGeometry() : base("Display Geometry", "DispGeo", "Display geometry in the viewport with custom color and width. t provides a fade effect by time.", "Woodpecker", "Display")
        {
        }
        // private bool _flatShading = true;
        // protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        // {
        //     base.AppendAdditionalComponentMenuItems(menu);
        //     Menu_AppendSeparator(menu);
        //     Menu_AppendItem(menu, "Flat Shading", ActivativeFlatShading, true, _flatShading);
        // }
        // private void ActivativeFlatShading(object sender, EventArgs e)
        // {
        //     _flatShading = !_flatShading;
        // }
        public override Guid ComponentGuid => new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to display.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Color", "Col", "Color of the geometry.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No outputs needed for display component
        }
        private DisplayGeometry _displayGeo;
        protected override void BeforeSolveInstance()
        {
            //_displayGeo = null; // Reset display geometry before each solution
            _iGs = new DataTree<GeometryBase>();
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _displayGeo = null; // Reset display geometry before each solution
            if(!DA.GetDataTree<IGH_GeometricGoo>(0, out var iGs))
            {
                return;
            }
            Color col = Color.Black;

            DA.GetData("Color", ref col);

            int width = 1;
            DA.GetDataTree<GH_Number>(2, out var t);
            var geoTree = new DataTree<GeometryBase>();
            DataUtil.GH_Structure2GH_DataTree(iGs, ref geoTree);

            // var geoTree = new DataTree<GeometryBase>();
            // for (int i = 0; i < iGs.Branches.Count; i++)
            // {
            //     geoTree.AddRange(iGs.Branches[i].ConvertAll(g => g.ScriptVariable() as GeometryBase), new GH_Path(i));
            // }
            var tTree = new DataTree<double>();
            DataUtil.GH_Structure2GH_DataTree(t, ref tTree);
            
            // var tTree = new DataTree<double>();
            // for (int i = 0; i < t.Branches.Count; i++)
            // {
            //     tTree.AddRange(t.Branches[i].ConvertAll(n => n.Value), new GH_Path(i));
            // }
            _iGs = geoTree;
            _displayGeo = new DisplayGeometry(geoTree, col, width, tTree);
            _displayMesh = _displayGeo.GetDisplayMesh();
        }
        public override BoundingBox ClippingBox => _displayGeo?.ClippingBox ?? BoundingBox.Empty;
        private DataTree<Color> _iColors => this._displayGeo?.GetColors() ?? new DataTree<Color>();
        private DataTree<double> _transparency => this._displayGeo?.GetTransparency() ?? new DataTree<double>();
        private DataTree<GeometryBase> _iGs;
        private DataTree<Mesh> _displayMesh;
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            for (int i = 0; i < _iColors.BranchCount; i++)
            {
                for (int j = 0; j < _iColors.Branch(i).Count; j++)
                {
                    var LocTransparency = _transparency.Branch(i)[j];
                    var TempCol = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : _iColors.Branch(i)[j];
                    var DisplayMaterial = 
                            new Rhino.Display.DisplayMaterial(TempCol, LocTransparency);
                    
                    //var DisplayMaterial = _flatShading ? 
                    //        new Rhino.Display.DisplayMaterial(TempCol, TempCol, TempCol, TempCol, 0, LocTransparency) :
                    //        new Rhino.Display.DisplayMaterial(TempCol, LocTransparency);
                    if (LocTransparency >= 0.99) continue; // Skip fully transparent geometry
                    foreach(var iM in _displayMesh.Branch(i))
                    {
                        args.Display.DrawMeshShaded(iM, DisplayMaterial);
                    }
                }
            }
        }
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            for (int i = 0; i < _iColors.BranchCount; i++)
            {
                for (int j = 0; j < _iColors.Branch(i).Count; j++)
                {
                    var LocTransparency = _transparency.Branch(i)[j];
                    if (LocTransparency >= 0.99) continue; // Skip fully transparent geometry
                    var TempCol = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : _iColors.Branch(i)[j];
                    var Geom = _iGs.Branch(i)[j];
                    if (Geom is Curve curve)
                    {
                        args.Display.DrawCurve(curve, TempCol, _displayGeo.GetWidth(false));
                    }
                }
            }
        }

    }
}
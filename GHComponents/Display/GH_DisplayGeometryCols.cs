using System.Collections.Generic;
using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using System.Linq;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Display geometry with custom colors in the viewport. Based on the pointer_t, the colors will be interpolated. Inputs include Linear, Geometry, Colors, Width, and Pointer_t.
    /// </summary>
    public class GH_DisplayGeometryCols : GH_Component
    {
        public GH_DisplayGeometryCols() : base("Display Geometry Cols", "DispGeoCols", "Display geometry with custom colors in the viewport. Based on the pointer_t, the colors will be interpolated.", "Woodpecker", "Display")
        {
        }

        public override Guid ComponentGuid => new Guid("b2c3d4e5-f6a7-8901-bcde-f234567890ab");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Geom_Cols;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Linear", "L", "Whether to use linear interpolation for colors. If false, it will use exponential interpolation.", GH_ParamAccess.item, true);
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to display.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Colors", "Cols", "Colors for the geometry. The structure should match the Geometry input.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Width", "W", "Width of the geometry display.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for color interpolation (0 to 1).", GH_ParamAccess.tree, 1.0);
        }
        public override BoundingBox ClippingBox => _displayGeo.Aggregate(new BoundingBox(), (acc, dg) => { acc.Union(dg.ClippingBox); return acc; });
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No outputs needed for display component
        }
        private List<DisplayGeometryCols> _displayGeo;
        protected override void BeforeSolveInstance()
        {
            _displayGeo = new List<DisplayGeometryCols>(); // Reset display geometry before each solution
        }
        private List<Color> _currentCol;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool linear = true;
            DA.GetData("Linear", ref linear);
            DA.GetDataTree<IGH_GeometricGoo>(1, out var iGs);
            DA.GetDataTree<GH_Colour>(2, out var iCols);
            int width = 1;
            DA.GetData("Width", ref width);
            DA.GetDataTree<GH_Number>(4, out var t);

            var iColsList = new List<List<Color>>();
            var iGeomsList = new List<List<GeometryBase>>();
            var tList = new List<double>();
            for (int i = 0; i < iGs.Branches.Count; i++)
            {
                iGeomsList.Add(new List<GeometryBase>(iGs.Branches[i].Select(x => x.ScriptVariable() as GeometryBase)));
                var colSelected = i >= iCols.Branches.Count ? iCols.Branches.Count - 1 : i;
                iColsList.Add(new List<Color>(iCols.Branches[colSelected].Select(x => x.Value)));
                var tSelected = i >= t.Branches.Count ? t.Branches.Count - 1 : i;
                tList.Add(t.Branches[tSelected].First().Value);
            }
            _displayGeo = new List<DisplayGeometryCols>();
            for (int i = 0; i < iGeomsList.Count; i++)
            {
                _displayGeo.Add(new DisplayGeometryCols(iGeomsList[i], iColsList[i], width, linear));
            }

            _currentCol = new List<Color>();
            for(int i = 0; i < tList.Count; i++)
            {
                _currentCol.Add(_displayGeo[i].GetColor(tList[i]));
            }
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            for(int i = 0; i < _displayGeo.Count; i++)
            {
                var displayColor = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : _currentCol[i];
                var RhinoMaterial = new Rhino.Display.DisplayMaterial(displayColor) { Transparency = 1 - displayColor.A / 255.0 };
                foreach (var g in _displayGeo[i].GetGeoms())
                {
                    if (g is Mesh mesh)
                        args.Display.DrawMeshShaded(mesh, RhinoMaterial);
                    else if (g is Brep brep)
                        foreach(var M in Mesh.CreateFromBrep(brep, MeshingParameters.Default))
                            args.Display.DrawMeshShaded(M, RhinoMaterial);
                    else if (g is Extrusion iE)
                    {
                        var iEB = iE.ToBrep();
                        foreach (var M in Mesh.CreateFromBrep(iEB, MeshingParameters.Default))
                        {
                            args.Display.DrawMeshShaded(M, RhinoMaterial);
                        }
                    }
                }
            }
        }
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            for(int i = 0; i < _displayGeo.Count; i++)
            {
                var displayColor = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : _currentCol[i];
                foreach (var g in _displayGeo[i].GetGeoms())
                {
                    if (g is Curve curve)
                        args.Display.DrawCurve(curve, displayColor, _displayGeo[i].GetWidth());
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using Rhino.UI.DialogPanels;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.Geometry.Display
{
    public class DisplayGeometry
    {
        private DataTree<Color> _iColors;
        private DataTree<GeometryBase> _iGs;
        private BoundingBox _clip;
        private int _width;
        private DataTree<double> _transparency;
        public void initialised()
        {
            _iColors = new DataTree<Color>();
            _iGs = new DataTree<GeometryBase>();
            _transparency = new DataTree<double>();
            _clip = new BoundingBox();
        }
        public BoundingBox ClippingBox => _clip;
        public DataTree<Color> GetColors() => _iColors;
        public DataTree<GeometryBase> GetGeoms () => _iGs;
        public DataTree<double> GetTransparency () => _transparency;
        public int GetWidth(bool ChangeFromView) => 
        ChangeFromView ? 
        GetWorldWidthFromScreenWidth(_width, _clip.Center) : 
        _width;
        public DisplayGeometry(DataTree<GeometryBase> iGs, Color Col, int Width, DataTree<double> t)
        {
            initialised();
            _iGs = iGs;

            t = DataUtil.AlignDataTree(iGs, t);
            _width = Width <= 0 ? 1 : Width;

            _clip = iGs.AllData().Aggregate(_clip, (acc, g) => { acc.Union(g.GetBoundingBox(true)); return acc; });

            var tTree = new DataTree<double>();
            for (int i = 0; i < t.BranchCount; i++)
            {
                for (int j = 0; j < t.Branch(i).Count; j++)
                {
                    var num = t.Branch(i)[j] == 0 ? 1e-2 : t.Branch(i)[j];
                    tTree.Add(num, t.Path(i));
                    _transparency.Add(1 - (Col.A / 255.0) * num, t.Path(i));
                    _iColors.Add(Color.FromArgb((int)Math.Round(Col.A * num), Col.R, Col.G, Col.B), t.Path(i));
                }
            }
        }
        public static int GetWorldWidthFromScreenWidth(int lineWidth, Point3d referencePt)
        {
            var avp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            if(!avp.GetWorldToScreenScale(referencePt, out var pixelsPerUnit))
            {
                return lineWidth;
            }
            if(pixelsPerUnit <= 0 || double.IsNaN(pixelsPerUnit) || double.IsInfinity(pixelsPerUnit))
                return lineWidth;
            return Math.Max(1, (int)Math.Round(lineWidth / pixelsPerUnit));
        }
        public DataTree<Curve> GetEdge()
        {
            var iGsCrvs = new DataTree<Curve>();
            for(int i = 0; i < _iGs.BranchCount; i++)
            {
                var pathSetting = _transparency.Path(i);
                for(int j = 0; j < _iGs.Branch(i).Count; j++)
                {
                    if(_iGs.Branch(i)[j] is Extrusion _iGE)
                    {
                        var _iE2B = _iGE.ToBrep();
                        iGsCrvs.AddRange(_iE2B.GetWireframe(-1), pathSetting);
                    }
                    if(_iGs.Branch(i)[j] is Brep _iGB)
                    {
                        iGsCrvs.AddRange(_iGB.GetWireframe(-1), pathSetting);
                    }
                    if(_iGs.Branch(i)[j] is Mesh _iGM)
                    {
                        var Crv = new List<Curve>();
                        for (int k = 0; k < _iGM.TopologyEdges.Count; k++)
                        {
                            if (_iGM.TopologyEdges.IsEdgeUnwelded(i))
                                Crv.Add(
                                    new LineCurve(_iGM.TopologyEdges.EdgeLine(i))
                                );
                        }
                        iGsCrvs.AddRange(Crv, pathSetting);
                    }
                    if(_iGs.Branch(i)[j] is Curve iGC)
                        iGsCrvs.Add(iGC, pathSetting);
                }
            }

            return iGsCrvs;
        }
        public DataTree<Mesh> GetDisplayMesh()
        {
            var outM = new DataTree<Mesh>();

            for(int i = 0; i < _iGs.BranchCount; i++)
            {
                for(int j = 0; j < _iGs.Branch(i).Count; j++)
                {
                    var Geom = _iGs.Branch(i)[j];
                    if (Geom is Mesh iM)
                    {
                        outM.Add(iM, _iGs.Path(i));
                    }
                    else if (Geom is Brep iB)
                        foreach (var M in Mesh.CreateFromBrep(iB, MeshingParameters.FastRenderMesh))
                        {
                            outM.Add(M, _iGs.Path(i));
                        }
                    else if (Geom is Extrusion iE)
                    {
                        var iEB = iE.ToBrep();
                        foreach (var M in Mesh.CreateFromBrep(iEB, MeshingParameters.FastRenderMesh))
                        {
                            outM.Add(M, _iGs.Path(i));
                        }
                    }
                }
            }
            return outM;
        }
    }
}
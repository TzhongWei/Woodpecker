using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Display
{
    public class DisplayGeometryContent : DisplayContentAbstract<GeometryBase>
    {
        protected readonly GeometryBase m_Geometry;
        public bool IsPoint => m_Geometry is Rhino.Geometry.Point;
        public string Type => m_Geometry?.GetType().Name ?? string.Empty;
        protected List<Curve> _wire;
        protected List<Mesh> _mesh;

        public List<Curve> GeometryWireFrame
        {
            get
            {
                var crvs = new List<Curve>(_wire ?? new List<Curve>());
                if (DoSilhouette)
                    crvs.AddRange(GetSilhouette());
                if(dashType != DashType.Continuous)
                {
                    return crvs.SelectMany(x =>
                    {
                        var crvDisplay = new CurveDisplay(x, dashType);
                        return crvDisplay.GetCurvesByDashType();
                    }).ToList();
                    
                }
                else
                    return crvs;
            }
        }
        public DashType dashType = DashType.Continuous;

        public IReadOnlyList<Mesh> GeometryMesh => _mesh;
        public bool DoSilhouette = false;
        public override BoundingBox ClippingBox => _clip;
        public override bool IsValid => m_Geometry != null && m_Geometry.IsValid;
        private BoundingBox _clip;
        private int _width = 1;
        public bool WidthChangeFromView = false;
        public int Width
        {
            get => WidthChangeFromView ? GetWorldWidthFromScreenWidth(_width, _clip.Center) : _width;
            set => _width = value > 0 ? value : 1;
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

        public DisplayGeometryContent(GeometryBase geometry, Color colour)
            : base(geometry, colour)
        {
            m_Geometry = geometry;
            Initialised();
        }
        protected virtual void Initialised()
        {
            if (!IsValid)
            {
                _mesh = new List<Mesh>();
                _wire = new List<Curve>();
                _clip = BoundingBox.Empty;
                return;
            }

            _mesh = GetMeshes();
            _wire = GetEdges();
            _clip = m_Geometry.GetBoundingBox(true);
        }
        protected List<Curve> GetSilhouette()
        {
            if (!IsValid || m_Geometry is Curve || !DoSilhouette)
                return new List<Curve>();

            return DisplayUtil.DisplaySilhouette(m_Geometry);
        }

        protected List<Mesh> GetMeshes()
        {
            var iGsMesh = new List<Mesh>();
            if (m_Geometry is Mesh _iGM)
            {
                iGsMesh.Add(_iGM);
            }
            else if (m_Geometry is Brep _iB)
            {
                var meshes = Mesh.CreateFromBrep(
                    _iB,
                    MeshingParameters.FastRenderMesh);

                if (meshes != null)
                    iGsMesh.AddRange(meshes);
            }
            else if (m_Geometry is Extrusion _iE)
            {
                var iE2B = _iE.ToBrep();
                var meshes = Mesh.CreateFromBrep(
                    iE2B,
                    MeshingParameters.FastRenderMesh);

                if (meshes != null)
                    iGsMesh.AddRange(meshes);
            }
            else if (m_Geometry is SubD iSb)
            {
                iGsMesh.Add(Mesh.CreateFromSubD(iSb, 5));
            }

            return iGsMesh;
        }

        protected List<Curve> GetEdges()
        {
            var iGsCrvs = new List<Curve>();
            if (m_Geometry is Extrusion _iGE)
            {
                var _iE2B = _iGE.ToBrep();
                iGsCrvs.AddRange(_iE2B.GetWireframe(-1));
            }
            else if (m_Geometry is Brep _iGB)
            {
                iGsCrvs.AddRange(_iGB.GetWireframe(-1));
            }
            else if (m_Geometry is Mesh _iGM)
            {
                var Crv = new List<Curve>();
                for (int k = 0; k < _iGM.TopologyEdges.Count; k++)
                {
                    if (_iGM.TopologyEdges.IsEdgeUnwelded(k))
                        Crv.Add(
                            new LineCurve(_iGM.TopologyEdges.EdgeLine(k))
                        );
                }
                iGsCrvs.AddRange(Crv);
            }
            else if (m_Geometry is Surface _iGS)
            {
                var _iS2B = _iGS.ToBrep();
                iGsCrvs.AddRange(_iS2B.GetWireframe(-1));
            }
            else if (m_Geometry is SubD _iGSb)
            {
                iGsCrvs.AddRange(_iGSb.Edges.Select(x => x.ToNurbsCurve(true)));
            }
            else if (m_Geometry is Curve _iGC)
            {
                iGsCrvs.Add(_iGC);
            }
            return iGsCrvs;
        }
    }
}

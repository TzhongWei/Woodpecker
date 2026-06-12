using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Woodpecker.Animation.Geometry.Display
{
    /// <summary>
    /// Defines the temporary clipping state applied while a target render
    /// pipeline draws. It does not contain or process geometry itself.
    /// </summary>
    public sealed class DisplayClippingContent : DisplayGeometryContent
    {
        private readonly List<Plane> _clippingPlanes;
        private readonly List<List<Mesh>> _sectionFillMeshesByPlane =
            new List<List<Mesh>>();
        private readonly List<List<Curve>> _sectionCurvesByPlane =
            new List<List<Curve>>();

        public DisplayClippingContent(
            GeometryBase geometry, Color colour,
            IEnumerable<Plane> clippingPlanes,
            bool showSectionFill = true,
            bool showSectionWire = false) : 
            base(geometry, colour)
        {
            _clippingPlanes = clippingPlanes?
                .Where(plane => plane.IsValid)
                .Select(plane => new Plane(plane))
                .ToList() ?? new List<Plane>();

            ShowSectionFill = showSectionFill;
            ShowSectionWire = showSectionWire;
            Initialised();
            BuildSectionGeometry();
        }
        public IReadOnlyList<Plane> ClippingPlanes => _clippingPlanes;
        public IReadOnlyList<Mesh> GetSectionFillMeshes(
            int clippingPlaneIndex)
        {
            if (clippingPlaneIndex < 0 ||
                clippingPlaneIndex >=
                    _sectionFillMeshesByPlane.Count)
            {
                return new Mesh[0];
            }

            return _sectionFillMeshesByPlane[
                clippingPlaneIndex];
        }
        public IReadOnlyList<Curve> GetSectionCurves(
            int clippingPlaneIndex)
        {
            if (clippingPlaneIndex < 0 ||
                clippingPlaneIndex >=
                    _sectionCurvesByPlane.Count)
            {
                return new Curve[0];
            }

            return _sectionCurvesByPlane[
                clippingPlaneIndex];
        }
        public bool ShowSectionFill { get; set; }
        public bool ShowSectionWire { get; set; }
        public override bool IsValid =>
            m_Geometry != null && m_Geometry.IsValid &&
            _clippingPlanes != null &&
            _clippingPlanes.Count > 0;

        private void BuildSectionGeometry()
        {
            _sectionFillMeshesByPlane.Clear();
            _sectionCurvesByPlane.Clear();

            for (var i = 0; i < _clippingPlanes.Count; i++)
            {
                _sectionFillMeshesByPlane.Add(new List<Mesh>());
                _sectionCurvesByPlane.Add(new List<Curve>());
            }

            if ((!ShowSectionFill && !ShowSectionWire) ||
                !IsValid ||
                GeometryMesh == null ||
                GeometryMesh.Count == 0)
                return;

            var combinedMesh = BuildCombinedMesh();
            if (combinedMesh == null)
                return;

            var tolerance = GetSectionTolerance(combinedMesh);

            for (var planeIndex = 0;
                planeIndex < _clippingPlanes.Count;
                planeIndex++)
            {
                var plane = _clippingPlanes[planeIndex];
                var sectionMeshes =
                    _sectionFillMeshesByPlane[planeIndex];
                var closedCurves = GetClosedSectionCurves(
                    combinedMesh,
                    plane,
                    tolerance);

                if (closedCurves.Count == 0)
                    continue;

                if (ShowSectionWire)
                {
                    _sectionCurvesByPlane[
                        planeIndex].AddRange(
                            closedCurves);
                }

                if (!ShowSectionFill)
                    continue;

                var sectionBreps = CreateSectionBreps(
                    closedCurves,
                    plane,
                    tolerance);

                if (sectionBreps.Count == 0)
                    continue;

                foreach (var sectionBrep in sectionBreps)
                {
                    var meshes = Mesh.CreateFromBrep(
                        sectionBrep,
                        MeshingParameters.FastRenderMesh);

                    if (meshes == null)
                        continue;

                    sectionMeshes.AddRange(
                        meshes.Where(mesh =>
                            mesh != null &&
                            mesh.IsValid));
                }
            }

            combinedMesh.Dispose();
        }

        private Mesh BuildCombinedMesh()
        {
            var combinedMesh = new Mesh();

            foreach (var geometryMesh in GeometryMesh)
            {
                if (geometryMesh != null &&
                    geometryMesh.IsValid)
                {
                    combinedMesh.Append(geometryMesh);
                }
            }

            if (!combinedMesh.IsValid ||
                combinedMesh.Faces.Count == 0)
            {
                combinedMesh.Dispose();
                return null;
            }

            combinedMesh.Vertices.CombineIdentical(
                true,
                true);
            combinedMesh.Weld(System.Math.PI);
            combinedMesh.UnifyNormals();
            combinedMesh.Compact();

            return combinedMesh;
        }

        private static double GetSectionTolerance(
            Mesh mesh)
        {
            var documentTolerance =
                RhinoDoc.ActiveDoc?.ModelAbsoluteTolerance ??
                1e-6;
            var diagonal =
                mesh.GetBoundingBox(true).Diagonal.Length;
            var scaleTolerance =
                diagonal > 0
                    ? diagonal * 1e-8
                    : 0;

            return System.Math.Max(
                documentTolerance,
                scaleTolerance);
        }

        private static List<Curve> GetClosedSectionCurves(
            Mesh mesh,
            Plane plane,
            double tolerance)
        {
            var sections = Intersection.MeshPlane(
                mesh,
                plane);

            if (sections == null ||
                sections.Length == 0)
            {
                return new List<Curve>();
            }

            var fragments = sections
                .Where(section =>
                    section != null &&
                    section.Count >= 2 &&
                    section.Length > tolerance)
                .Select(section =>
                    (Curve)new PolylineCurve(section))
                .ToList();

            if (fragments.Count == 0)
                return new List<Curve>();

            var joinedCurves = Curve.JoinCurves(
                fragments,
                tolerance,
                false);
            var closedCurves = new List<Curve>();

            foreach (var joinedCurve in joinedCurves)
            {
                if (joinedCurve == null ||
                    !joinedCurve.IsValid ||
                    joinedCurve.GetLength() <= tolerance)
                {
                    continue;
                }

                var planarCurve = Curve.ProjectToPlane(
                    joinedCurve,
                    plane);
                if (planarCurve == null ||
                    !planarCurve.IsValid)
                {
                    continue;
                }

                if (!planarCurve.IsClosed &&
                    !planarCurve.MakeClosed(tolerance))
                {
                    continue;
                }

                if (planarCurve.IsClosed)
                    closedCurves.Add(planarCurve);
            }

            return closedCurves;
        }

        private static List<Brep> CreateSectionBreps(
            List<Curve> closedCurves,
            Plane plane,
            double tolerance)
        {
            var sectionBreps = new List<Brep>();

            using (var regions =
                Curve.CreateBooleanRegions(
                    closedCurves,
                    plane,
                    false,
                    tolerance))
            {
                if (regions != null)
                {
                    for (var i = 0;
                        i < regions.RegionCount;
                        i++)
                    {
                        var boundaries =
                            regions.RegionCurves(i);
                        if (boundaries == null ||
                            boundaries.Length == 0)
                        {
                            continue;
                        }

                        var breps =
                            Brep.CreatePlanarBreps(
                                boundaries,
                                tolerance);
                        if (breps != null)
                            sectionBreps.AddRange(breps);
                    }
                }
            }

            // Boolean regions may reject imperfect but otherwise usable
            // loops. Keep the simpler planar-brep method as a fallback.
            if (sectionBreps.Count == 0)
            {
                var fallbackBreps =
                    Brep.CreatePlanarBreps(
                        closedCurves,
                        tolerance);
                if (fallbackBreps != null)
                    sectionBreps.AddRange(fallbackBreps);
            }

            return sectionBreps;
        }
    }
}

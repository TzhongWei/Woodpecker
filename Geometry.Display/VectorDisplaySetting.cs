
using Grasshopper;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Drawing;
using System;
using System.Linq;
using Grasshopper.Kernel;

namespace Woodpecker.Animation.Geometry.Display
{
    public enum VectorDisplayType
    {
        DirectionArrow,
        HollowArrow,
        SolidArrow
    }
    public class VectorDisplaySetting
    {
        public double Length { get; set; }
        public int Width { get; set; }
        public double ArrowheadSize { get; set; }
        // public VectorDisplayType DisplayType { get; set; }
        public double ArrowRelativeSize {get; set;}
        [Obsolete]
        public VectorDisplay VectorDisplay { get; set; }
        private Color _colour;
        public Color Colour => _colour;
        public static VectorDisplaySetting Unset => new VectorDisplaySetting();
        public VectorDisplaySetting()
        {
            Length = 1.0;
            ArrowheadSize = 2;
            ArrowRelativeSize = 1;
            Width = 1;
            _colour = Color.Black;
            // DisplayType = VectorDisplayType.DirectionArrow;
            //_arrowGeometries = new List<GeometryBase>();
        }
        public VectorDisplaySetting(double length, double arrowheadSize, int width, Color color)
        {
            Length = length;
            ArrowheadSize = arrowheadSize;
            Width = width;
            _colour = color;
            // DisplayType = VectorDisplayType.DirectionArrow;
            // _arrowGeometries = new List<GeometryBase>();
        }
        // private List<GeometryBase> _arrowGeometries = new List<GeometryBase>(); // Store the arrow geometries, need to be oriented on to a plane.
        /// <summary>
        /// Indicates whether the arrowhead should be drawn as a solid shape (e.g., a filled triangle) or as a hollow shape (e.g., an open triangle).
        /// This property is relevant when drawing the arrowhead in the DrawViewportWires or DrawViewportMeshes methods of the GH_VectorDisplay related class. 
        /// </summary>
        // public bool IsSolidArrow => DisplayType == VectorDisplayType.SolidArrow;
        // public List<GeometryBase> DrawArrowhead(Plane plane = new Plane())
        // {
        //     if (plane == new Plane())
        //     {
        //         var doc = Rhino.RhinoDoc.ActiveDoc;
        //         var normalDirectionOfVector = doc.Views.ActiveView.ActiveViewport.CameraDirection;
        //         plane = new Plane(VectorDisplay.ArrowheadLocation, normalDirectionOfVector);
        //         plane.YAxis = VectorDisplay.Direction;
        //     }
        //     else
        //     {
        //         plane.Origin = VectorDisplay.ArrowheadLocation;
        //     }
        //     var OrientedTS = Transform.PlaneToPlane(Plane.WorldXY, plane);
        //     DrawDirectionArrow();
        //     _arrowGeometries = _arrowGeometries.Select(x => { x.Transform(OrientedTS); return x; }).ToList();
        //     return _arrowGeometries;
        // }
        // private void DrawDirectionArrow()
        // {
        //     Implementation for drawing a simple direction arrow
        //     var ArrDir1 = Vector3d.YAxis;
        //     ArrDir1.Rotate(0.15 * Math.PI, Vector3d.ZAxis);
        //     var ArrDir2 = Vector3d.YAxis;
        //     ArrDir2.Rotate(-0.15 * Math.PI, Vector3d.ZAxis);
        //     var arrowheadLocation = new Point3d(0, 0, 0);
        //     var Line1 = new LineCurve(arrowheadLocation, ArrDir1 * ArrowheadSize + arrowheadLocation);
        //     var Line2 = new LineCurve(arrowheadLocation, ArrDir2 * ArrowheadSize + arrowheadLocation);
        //     var Line3 = new LineCurve(ArrDir1 * ArrowheadSize + arrowheadLocation, ArrDir2 * ArrowheadSize + arrowheadLocation);
        //     var JointCrv = Curve.JoinCurves(new Curve[] { Line1, Line2, Line3 })[0];

        //     switch (DisplayType)
        //     {
        //         case VectorDisplayType.DirectionArrow:
        //             // Draw a line from the location in the direction of the vector with the specified length
        //             // Add an arrowhead at the end of the line
        //             _arrowGeometries.Add(Line1);
        //             _arrowGeometries.Add(Line2);
        //             break;
        //         case VectorDisplayType.HollowArrow:
        //             // Draw a hollow arrow (e.g., a line with an open triangle at the end)
        //             _arrowGeometries.Add(JointCrv);
        //             break;
        //         case VectorDisplayType.SolidArrow:
        //             // Draw a solid arrow (e.g., a line with a filled triangle at the end)
        //             var arrowMesh = new Mesh();
        //             arrowMesh.Vertices.AddVertices(new Point3d[]{arrowheadLocation, ArrDir1 * ArrowheadSize + arrowheadLocation, ArrDir2 * ArrowheadSize + arrowheadLocation});
        //             arrowMesh.Faces.AddFace(0, 1, 2);
        //             _arrowGeometries.Add(arrowMesh);
        //             break;
        //     }
        // }
        [Obsolete]
        public Curve DrawArrowCurve(Curve baseCurve)
        {
            baseCurve.ClosestPoint(this.VectorDisplay.ArrowheadLocation, out double param);
            var pointOnCrv = baseCurve.PointAt(param);
            if(pointOnCrv.DistanceTo(this.VectorDisplay.ArrowheadLocation) > 1e-2)
            {
                throw new Exception("Arrowhead location is too far from the base curve, cannot draw arrow.");
            }
            if(param == baseCurve.Domain.Min || param - Length < baseCurve.Domain.Min)
            {
                return DrawArrowLine();
            }
            var St = param;
            var En = param - Length;
            var arrowLine = baseCurve.Trim(new Interval(St, En));
            return arrowLine;
        }
        /// <summary>
        /// Draw linear body of the arrow, which is a line from the location in the direction of the vector with the specified length.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Curve DrawArrowLine()
        {
            var arrowLine = new LineCurve(this.VectorDisplay.ArrowheadLocation - this.VectorDisplay.Direction * Length, this.VectorDisplay.ArrowheadLocation);
            return arrowLine;
        }
        // public void DrawArrowhead(IGH_PreviewArgs args)
        // {
        //     var arrowGeometries = GetArrowGeometries();
        //     foreach (var geom in arrowGeometries)
        //     {
        //         if (geom is Curve curve)
        //         {
        //             args.Display.DrawCurve(curve, CurrentColor, Width);
        //         }
        //         else if (geom is Brep brep)
        //         {
        //             args.Display.DrawBrepShaded(brep, new Rhino.Display.DisplayMaterial(CurrentColor));
        //         }
        //     }
        // }
    }
}
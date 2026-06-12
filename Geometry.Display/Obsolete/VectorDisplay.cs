using Eto.Forms;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Woodpecker.Animation.Geometry.Display
{
    [Obsolete]
    public class VectorDisplay
    {
        private Point3d _arrowheadLocation;
        private Vector3d _direction;
        private VectorDisplaySetting _vectorDisplaySetting = new VectorDisplaySetting();
        private List<Curve> _body;
        public void SetVectorDisplaySetting(VectorDisplaySetting setting)
        {
            setting.VectorDisplay = this;
            _vectorDisplaySetting = setting;
        }
        private VectorDisplay(Point3d arrowheadLocation, Vector3d direction, VectorDisplaySetting vectorDisplaySetting)
        {
            _arrowheadLocation = arrowheadLocation;
            if(!direction.Unitize())
            throw new Exception($"Vector {direction} cannot be unitised");
            _direction = direction;
            SetVectorDisplaySetting(vectorDisplaySetting);
            _body = new List<Curve>();
        }
        public Vector3d Direction => _direction;
        public Point3d ArrowheadLocation => _arrowheadLocation;
        public static void DrawCurveArrow(GH_Component component, IGH_PreviewArgs args, CurveDisplay curveDisplay, double Ct, VectorDisplaySetting setting, double t = 1.0)
        {
            var Crv = curveDisplay.O_Curve;
            var ptOnCrv = Crv.PointAt(t);
            var tangentOnCrv = Crv.TangentAt(t);
            var vectorDisplay = new VectorDisplay(ptOnCrv, tangentOnCrv, setting);
            var settingCol = setting.Colour;
            var col = component.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.FromArgb((int)Math.Round(t * settingCol.A),
            settingCol.R, settingCol.G, settingCol.B);
            var draw = setting.DrawArrowCurve(Crv);
            var newCrvDisplay = new CurveDisplay(draw, curveDisplay.GetDash());

            vectorDisplay._body.AddRange(newCrvDisplay.GetCurves());
            DisplayArrow(args, vectorDisplay, col);
        }
        public static void DrawCurveArrow(GH_Component component, IGH_PreviewArgs args, Curve Crv, double Ct, VectorDisplaySetting setting, double t = 1.0)
        {
            var ptOnCrv = Crv.PointAt(t);
            var tangentOnCrv = Crv.TangentAt(t);
            var vectorDisplay = new VectorDisplay(ptOnCrv, tangentOnCrv, setting);
            var settingCol = setting.Colour;
            var col = component.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.FromArgb((int)Math.Round(t * settingCol.A),
            settingCol.R, settingCol.G, settingCol.B);
            vectorDisplay._body.Add(setting.DrawArrowCurve(Crv));
            DisplayArrow(args, vectorDisplay, col);
        }
        public static void DrawLinearArrow(GH_Component component, IGH_PreviewArgs args, Point3d startPoint, Vector3d direction, double t = 1.0)
        {
            DrawLinearArrow(component, args, startPoint, direction, new VectorDisplaySetting(), t);
        }
        private static void DisplayArrow(IGH_PreviewArgs args, VectorDisplay vectorDisplay, Color col)
        {
            args.Display.DrawArrowHead(
                vectorDisplay.ArrowheadLocation,
                vectorDisplay.Direction,
                col,
                vectorDisplay._vectorDisplaySetting.ArrowheadSize,
                vectorDisplay._vectorDisplaySetting.ArrowRelativeSize
            );
            foreach (var crv in vectorDisplay._body)
            {
                args.Display.DrawCurve(
                    crv,
                    col,
                    vectorDisplay._vectorDisplaySetting.Width
                );
            }
        }
        public static void DrawLinearArrow(GH_Component component, IGH_PreviewArgs args, Point3d startPoint, Vector3d direction, VectorDisplaySetting setting, double t = 1.0)
        {
            var vectorDisplay = new VectorDisplay(startPoint, direction, setting);
            //ArrowHead = setting.DrawArrowhead();
            var settingCol = setting.Colour;
            var col = component.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.FromArgb((int)Math.Round(t * settingCol.A),
            settingCol.R, settingCol.G, settingCol.B);
            vectorDisplay._body.Add(setting.DrawArrowLine());
            DisplayArrow(args, vectorDisplay, col);

            // var vectorline = (setting.DrawArrowLine() as LineCurve).Line;
            // args.Display.DrawArrow(
            //     vectorline,
            //     Col,
            //     setting.ArrowheadSize,
            //     setting.ArrowRelativeSize
            // );

            // args.Display.DrawLine(vectorline, Col, setting.Width);
        }
    }
}
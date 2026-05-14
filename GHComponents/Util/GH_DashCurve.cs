using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using Grasshopper;
using Woodpecker.Animation.Geometry.Display;
using Rhino.Geometry;
using System.Drawing;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Woodpecker.Animation.Geometry;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DashCurve : GH_Component
    {
         public GH_DashCurve() : base("Dash Curve", "DashCrv", "Apply dash pattern to a curve.", "Woodpecker", "Util")
        {
        }
        public override Guid ComponentGuid => new Guid("2b48c7ac-b64c-4c12-84fb-8698789a1285");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to display with the selected dash pattern.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Dash Pattern", "D", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Dashed Curves", "DC", "List of dashed curves.", GH_ParamAccess.list);
        }
        private CurveDisplay _curveDisplay;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            DashType dashPattern = "";
            DA.GetData("Curve", ref curve);
            DA.GetData("Dash Pattern", ref dashPattern);
            
            _curveDisplay = new CurveDisplay(curve, dashPattern);

            DA.SetDataList("Dashed Curves", _curveDisplay.GetCurves());
        }
    }
    [Obsolete]
    public class GH_DashCurve_OLD : GH_Component
    {
        public override GH_Exposure Exposure =>  GH_Exposure.hidden;
        public GH_DashCurve_OLD() : base("Dash Curve", "DashCrv", "Apply dash pattern to a curve.", "Woodpecker", "Util")
        {
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);


            // Add Value List for Dash Pattern
            Param_String in0Str = Params.Input[1] as Param_String;
            if (in0Str == null || in0Str.SourceCount > 0 || in0Str.PersistentDataCount > 0) return;
            Attributes.PerformLayout();
            int x = (int)in0Str.Attributes.Pivot.X - 150;
            int y = (int)in0Str.Attributes.Pivot.Y;
            GH_ValueList valueList = new GH_ValueList();

            valueList.CreateAttributes();
            valueList.Attributes.Pivot = new PointF(x, y);
            valueList.ListItems.Clear();

            // Continuous ""
            // Dot "0.1 1.0"
            // Dash    "1.0"
            // DashDot "1.0 1.0 0.1 1.0"
            // Hidden  "1.0 1.0"
            List<GH_ValueListItem> Type = new List<GH_ValueListItem>
            {
                new GH_ValueListItem("Continuous", "\"\""),
                new GH_ValueListItem("Dot", "\"0.1 1.0\""),
                new GH_ValueListItem("Dash", "\"1.0\""),
                new GH_ValueListItem("DashDot", "\"1.0 1.0 0.1 1.0\""),
                new GH_ValueListItem("Hidden", "\"1.0 1.0\"")
            };
            valueList.ListItems.AddRange(Type);
            document.AddObject(valueList, false);
            in0Str.AddSource(valueList);
        }
        public override Guid ComponentGuid => new Guid("d1c9e5b8-9c3a-4f0e-8c7b-2a1f5e6d7c8e");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to display with the selected dash pattern.", GH_ParamAccess.item);
            pManager.AddTextParameter("Dash Pattern", "D", "Dash pattern as a comma-separated string of numbers (e.g., \"0.1 1.0\" for 0.1 units on, 1.0 units off).", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "Col", "Color of the dashed curve.", GH_ParamAccess.item, System.Drawing.Color.Black);
            pManager.AddIntegerParameter("Width", "W", "Width of the dashed curve.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Dashed Curves", "DC", "List of dashed curves.", GH_ParamAccess.list);
            pManager.AddGenericParameter("CurveDisplay", "CD", "The object from CurveDisplay Class", GH_ParamAccess.item);
        }
        private List<Curve> _displayCurves = new List<Curve>();
        private int _width = 1;
        private Color _color = Color.Black;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            string dashPattern = "";
            System.Drawing.Color color = System.Drawing.Color.Black;
            int width = 1;
            DA.GetData("Curve", ref curve);
            DA.GetData("Dash Pattern", ref dashPattern);
            DA.GetData("Color", ref color);
            DA.GetData("Width", ref width);
            this._color = color;
            this._width = width;

            var curveDisplay = new CurveDisplay(curve, dashPattern);
            this._displayCurves = curveDisplay.GetCurves();

            _clip = BoundingBox.Union(_clip, curve.GetBoundingBox(false));

            DA.SetDataList("Dashed Curves", this._displayCurves);
            DA.SetData("CurveDisplay", curveDisplay);
        }
        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _width = 1;
            _color = Color.Black;
            _displayCurves.Clear();
        }
        public override BoundingBox ClippingBox => _clip;
        private BoundingBox _clip;
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            var displayColor = this.Attributes.Selected ? DisplayDefaultColour.SelectedColour : this._color;
            for (int i = 0; i < _displayCurves.Count; i++)
                args.Display.DrawCurve(_displayCurves[i], _color, _width);
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);
        }
    }
}

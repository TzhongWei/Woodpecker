using Woodpecker.Animation.Control.Timeline;
using Grasshopper.Kernel;
using Grasshopper;
using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Generate a range of intervals between two given intervals. Inputs include Timelines. Outputs include Max, Mid, and Min.
    /// </summary>
    public class GH_IntervalRange : GH_TimelineAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public GH_IntervalRange() : base("Interval Range", "IR", "Generate a range of intervals between two given intervals") { }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Interval_Range;
        public override Guid ComponentGuid => new Guid("3446981f-4b9d-47ea-9986-7a6a0b234b96");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timelines", "TLs", "List of timelines to analyze", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Max", "Max", "Maximum value in the range", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mid", "Mid", "Middle value in the range", GH_ParamAccess.item);
            pManager.AddNumberParameter("Min", "Min", "Minimum value in the range", GH_ParamAccess.item);
        }
        private double _min = 0.0;
        private double _max = 0.0;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var timelines = new List<Interval>();
            DA.GetDataList("Timelines", timelines);
            if (timelines.Count == 0) return;

            var min = double.MaxValue;
            var max = double.MinValue;

            TimelineSetting.IntervalRange(timelines, ref min, ref max);

            DA.SetData("Min", min);
            DA.SetData("Mid", (max + min) / 2);
            DA.SetData("Max", max);
        }

        protected override string ShowTimeSetupDescription()
        => "Min: " + _min + "\nMax: " + _max;
    }
    [Obsolete]
    /// <summary>
    /// Generate a range of intervals between two given intervals. Inputs include Timelines. Outputs include Max, Mid, and Min.
    /// </summary>
    public class GH_IntervalRange_old : GH_TimelineAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_IntervalRange_old() : base("Interval Range", "IR", "Generate a range of intervals between two given intervals") { }

        public override Guid ComponentGuid => new Guid("d1e2f3a4-b5c6-7890-1234-56789abcdef0");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timelines", "TLs", "List of timelines to analyze", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Max", "Max", "Maximum value in the range", GH_ParamAccess.item);
            pManager.AddNumberParameter("Min", "Min", "Minimum value in the range", GH_ParamAccess.item);
        }
        private double _min = 0.0;
        private double _max = 0.0;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var timelines = new List<Interval>();
            DA.GetDataList("Timelines", timelines);
            if (timelines.Count == 0) return;

            var min = double.MaxValue;
            var max = double.MinValue;

            TimelineSetting.IntervalRange(timelines, ref min, ref max);

            DA.SetData("Min", min);
            DA.SetData("Max", max);
        }

        protected override string ShowTimeSetupDescription()
        => "Min: " + _min + "\nMax: " + _max;
    }
}
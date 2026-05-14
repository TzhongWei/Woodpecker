using Woodpecker.Animation.Control.Timeline;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_CreateTimelineByAccumulatedTime : GH_TimelineAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public GH_CreateTimelineByAccumulatedTime() : base("Create Timelines By Accumulated Time Periods", "CTAT", "Create a timeline by accumulated time") { }

        public override Guid ComponentGuid => new Guid("e1f2a3b4-c5d6-7890-1234-56789abcdef0");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Start", "S", "Start time of the timeline", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("AccumulatedTime", "AT", "List of accumulated time values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Shift", "Sh", "Shift the timeline by a certain amount of time", GH_ParamAccess.list, 0.0);
            pManager.AddNumberParameter("Speed", "S", "Speed of the timeline, Sp > 0. Default is 1.", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timelines", "TLs", "Generated timeline based on accumulated time", GH_ParamAccess.list);
        }
        private List<Interval> _timeline = new List<Interval>();
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var AccumulatedTime = new List<double>();
            var shift = new List<double>();
            var start = 0.0;
            var speed = 1.0;
            DA.GetData("Start", ref start);
            DA.GetDataList("AccumulatedTime", AccumulatedTime);
            DA.GetDataList("Shift", shift);
            DA.GetData("Speed", ref speed);
            speed = speed <= 0 ? 1.0 : speed; // Ensure speed is
            
            var timelines = TimelineSetting.CreateTimelineByAccumulatedTime(start, AccumulatedTime, shift, speed);
            this._timeline = timelines;
            DA.SetDataList("Timelines", timelines);
            this.MessageSetup();
        }

        protected override string ShowTimeSetupDescription()
            => $"Generated Timeline: \n {_timeline.Aggregate("", (current, x) => current + TimelineSetting.TimelineDescription(x) + "\n")}";
    }
}
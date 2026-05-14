using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Commands;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_ShiftTimeline : GH_TimelineAbstract
    {
        public GH_ShiftTimeline():base(
            "Shift Timelines",
            "S_TL",
            "Shift and extend timeline intervals by applying per-segment delays, prolongation, and global speed scaling. Delay offsets the start of each segment, Prolong extends its duration, and Speed scales the overall timing progression."){}
        public override GH_Exposure Exposure => GH_Exposure.secondary; // adjust

        public override Guid ComponentGuid => new Guid("92baf4c7-5e4f-485a-a5e0-339acf17e4c2");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Base timeline interval to be subdivided and shifted.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Delay", "D", "Per-segment delay values. Each value offsets the start time of a segment.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Prolong", "Plong", "Per-segment prolongation values. Each value extends the duration of a segment.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Speed", "S", "Global speed factor applied to the entire timeline. S > 0, where 1.0 keeps original timing.", GH_ParamAccess.item, 1.0);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Shifted and prolonged timeline intervals.", GH_ParamAccess.list);
        }
        private List<Interval> _timelines;
        protected override string ShowTimeSetupDescription()
        {
            var MessageTL = "";
            foreach(var TL in _timelines)
            MessageTL += TimelineSetting.TimelineDescription(TL) + "\n";
            return MessageTL;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _timelines = new List<Interval>();
            var speed = 1.0;
            var TL = new Interval();
            var delay = new List<double>();
            var prolong = new List<double>();
            DA.GetData("Timeline", ref TL);
            DA.GetDataList("Delay", delay);
            if(!DA.GetDataList("Prolong", prolong))
                prolong = Enumerable.Repeat(0.0, delay.Count).ToList();
            DA.GetData("Speed", ref speed);
            speed = speed <= 0 ? 1.0 : speed; // Ensure speed is positive

            var resultTL = TimelineSetting.TimelineDelay(TL, delay, prolong, speed);
            _timelines = resultTL;
            this.MessageSetup();
            
            DA.SetDataList("Timeline", resultTL);
        }
    }
}

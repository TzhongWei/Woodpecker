using System;
using Grasshopper;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;


namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Create timeline intervals from start times and durations. Inputs include Start Time and Period. Outputs include Timeline.
    /// </summary>
    public class GH_CreateTimeline : GH_TimelineAbstract
    {
        public GH_CreateTimeline() : 
        base("Create Timelines", "C_TL", "Create timeline intervals from start times and durations.")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;  //create
        public override Guid ComponentGuid => new Guid("114f38fd-0ef7-4903-95fe-37dbeb62d5a2");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Start Time", "STime", "Start time values for each timeline interval.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Period", "P", "Duration values for each timeline interval.", GH_ParamAccess.list);
        }
        private List<Interval> _timelines = new List<Interval>();
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Timeline intervals created from start time and duration pairs.", GH_ParamAccess.list);
        }
        protected override string ShowTimeSetupDescription()
        {
            var Mes = "";
            foreach(var interval in _timelines)
            {
                Mes += TimelineSetting.TimelineDescription(interval) + "\n";
            }
            return Mes;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var stls = new List<double>();
            var pers = new List<double>();
            DA.GetDataList("Start Time", stls);
            DA.GetDataList("Period", pers);

            _timelines = new List<Interval>();
            DataUtil.AlignList(ref pers, ref stls);
            for(int i = 0; i < pers.Count; i++)
            {
                _timelines.Add(new Interval(stls[i], stls[i] + pers[i]));
            }
            this.MessageSetup();
            DA.SetDataList("Timeline", _timelines);
            
        }
    }
}

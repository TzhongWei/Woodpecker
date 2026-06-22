using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using Woodpecker.Animation.Control.Timeline;
using System.Drawing;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_IsTimelineChannelFinished : GH_TimelineAbstract
    {
        public GH_IsTimelineChannelFinished()
            : base(
                "Is Timeline Channel Finished",
                "IsTLCFinished",
                "Evaluate whether each timeline channel is waiting, active, or finished at the global time.")
        {
        }

        public override Guid ComponentGuid => new Guid("0a8792d8-f130-473d-b5ff-ac76128d3660");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter(
                "Timelines",
                "TLs",
                "Timeline channels to evaluate.",
                GH_ParamAccess.list);

            pManager.AddNumberParameter(
                "Global_T",
                "T",
                "Global time.",
                GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter(
                "Status",
                "S",
                "-1 = waiting, 0 = active, 1 = finished, -2 = invalid timeline.",
                GH_ParamAccess.list);

            pManager.AddBooleanParameter(
                "Finished Channel",
                "F",
                "True when the corresponding timeline channel has finished.",
                GH_ParamAccess.list);
        }
        protected override Bitmap Icon => Properties.Resources.GH_Is_Channel_Finish;

        private readonly List<int> _states = new List<int>();
        private double _globalT;

        protected override string ShowTimeSetupDescription()
        {
            if (_states.Count == 0)
                return $"Global T: {_globalT}\nChannels: None";

            var waiting = _states.Count(state => state == -1);
            var active = _states.Count(state => state == 0);
            var finished = _states.Count(state => state == 1);
            var invalid = _states.Count(state => state == -2);

            return $"Global T: {_globalT}\n" +
                   $"Waiting: {waiting} | Active: {active}\n" +
                   $"Finished: {finished} | Invalid: {invalid}";
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var timelines = new List<Interval>();
            var globalT = 0.0;

            _states.Clear();

            if (!DA.GetDataList("Timelines", timelines) ||
                !DA.GetData("Global_T", ref globalT) ||
                timelines.Count == 0)
            {
                _globalT = globalT;
                DA.SetDataList("Status", Array.Empty<int>());
                DA.SetDataList("Finished Channel", Array.Empty<bool>());
                MessageSetup();
                return;
            }

            _globalT = globalT;

            var finished = TimelineSetting.IsTimelineChannelFinished(
                timelines,
                globalT,
                out var states);

            _states.AddRange(states);

            DA.SetDataList("Status", states);
            DA.SetDataList("Finished Channel", finished);

            MessageSetup();
        }
    }
}

using Grasshopper.Kernel;
using GH_IO.Serialization;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using Woodpecker.Animation.Control.Timeline;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using System;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_IsActiveTimelineChannel : GH_TimelineAbstract
    {
        public GH_IsActiveTimelineChannel() : base(
            "Is Active Timeline Channel",
            "TL Channel",
            "Find the timeline channels active at the global time, optionally retaining the most recently completed channel.")
        {
        }

        public override Guid ComponentGuid => new Guid("a9589c33-c6e2-4db1-822c-2f6f4ff9e0fa");
        private bool _retainValue = true;
        private void RetainValue()
        {
            RecordUndoEvent("Toggle Retain Timeline Channel");
            _retainValue = !_retainValue;
            Attributes?.ExpireLayout();
            OnDisplayExpired(false);
            ExpireSolution(true);
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Retain Value", RetainValue);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timelines", "TLs", "List of timelines to switch channel", GH_ParamAccess.list);
            pManager.AddNumberParameter("Global_T", "T", "Global time", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Channel", "C", "Input global time in the timeline channels", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Timeline Channel", "TLC", "The selected timeline", GH_ParamAccess.list);
        }
        private List<Interval> _activechannel = new List<Interval>();
        private double _global_t;
        protected override string ShowTimeSetupDescription()
        {
            var activeTimeline = _activechannel.Count == 0
                ? "None"
                : string.Join("\n", _activechannel.Select(x => TimelineSetting.TimelineDescription(x, 4)));

            return $"Global T: {_global_t}\n" +
                   $"Retain Value: {_retainValue}\n" +
                   $"Active Timeline: {activeTimeline}";
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var timelines = new List<Interval>();
            var globalT = 0.0;

            _activechannel.Clear();

            if (!DA.GetDataList("Timelines", timelines) ||
                !DA.GetData("Global_T", ref globalT) ||
                timelines.Count == 0)
            {
                _global_t = globalT;
                DA.SetDataList("Channel", Array.Empty<int>());
                DA.SetDataList("Timeline Channel", Array.Empty<Interval>());
                MessageSetup();
                return;
            }

            _global_t = globalT;

            var activeChannels = new List<int>();
            TimelineSetting.IsActiveInChannel(
                timelines,
                globalT,
                _retainValue,
                ref activeChannels);

            activeChannels = activeChannels
                .Where(index => index >= 0 && index < timelines.Count)
                .Distinct()
                .ToList();

            _activechannel = activeChannels
                .Select(index => timelines[index])
                .ToList();

            DA.SetDataList("Channel", activeChannels);
            DA.SetDataList("Timeline Channel", _activechannel);

            MessageSetup();
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("RetainValue", _retainValue);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("RetainValue", ref _retainValue);
            return base.Read(reader);
        }
    }
}

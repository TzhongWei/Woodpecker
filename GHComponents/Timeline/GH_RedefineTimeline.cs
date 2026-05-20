using System;
using System.Collections.Generic;
using System.Drawing;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Redefine the time length of a time line from start or end. Inputs include Timeline, Period, and Speed. Outputs include Timeline.
    /// </summary>
    public class GH_RedefineTimeline : GH_TimelineAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_RedefineTimeline():base("Redefine Timelines", "ReTimeline", "Redefine the time length of a time line from start or end"){}

        public override Guid ComponentGuid => new Guid("f8ba9505-f87d-4228-99be-b815960c9b0b");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Base timeline interval to redefine.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Period", "P", "New period values used to rebuild the timeline from the selected side.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Speed", "S", "Speed factor applied to the redefined timeline duration.", GH_ParamAccess.item, 1.0);
        }
        private List<Color> stateColours = new List<Color>{Color.FromArgb(70, 167, 171, 110), Color.FromArgb(70, 144, 110, 171)}; //  rgba(167, 171, 110, 0.7)  rgba(144, 110, 171, 0.7)
        private bool _startOrend = true; 
        private List<Interval> _timelines;
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Redefined timeline intervals.", GH_ParamAccess.list);
        }
        private void ChangeState()
        {
            _startOrend = !_startOrend;
            var state = _startOrend ? 0 : 1;
            (this.Attributes as ButtonUIAttributesState).UpdateSelectedIndex(state);
            this.Attributes?.ExpireLayout();
            this.OnDisplayExpired(true);
            this.ExpireSolution(true);
        }
        public override void CreateAttributes()
        {
            this.m_attributes = new ButtonUIAttributesState(this, new List<string>{"Start", "End"}, ChangeState, stateColours, "Reset TL from", 0);
        }
        protected override string ShowTimeSetupDescription()
        {
            var Mes = "";
            Mes += _startOrend ? "reset from Start \n" : "reset from End \n";
            foreach(var interval in _timelines)
            {
                Mes += TimelineSetting.TimelineDescription(interval) + "\n";
            }
            return Mes;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _timelines = new List<Interval>();
            var tL = new Interval();
            var pers = new List<double>();
            var speed = 1.00;
            DA.GetData("Timeline", ref tL);
            DA.GetDataList("Period", pers);
            DA.GetData("Speed", ref speed);
            for(int i = 0; i < pers.Count; i++)
                _timelines.Add(TimelineSetting.RedefindTimeline(tL, _startOrend, pers[i], speed));
            this.MessageSetup();
            DA.SetDataList("Timeline", _timelines);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("State", _startOrend);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("State", ref _startOrend);
            return base.Read(reader);
        }
    }
}

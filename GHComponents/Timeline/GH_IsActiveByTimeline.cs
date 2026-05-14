using System;
using System.Data;
using System.Windows.Forms;
using Grasshopper.GUI.MRU;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class IsActiveByTimeline : GH_TimelineAbstract
    {
        public IsActiveByTimeline():base("Is Active in Timeline", "IsActive", "Test timeslot T is in a timeline and ouput a boolean value"){}
        public override Guid ComponentGuid => new Guid("e313420d-b2b2-49ac-980d-029f91ac4f96");
        private bool _invert;
        private void _toggle(object sender, EventArgs e)
        {
            _invert = !_invert;
            this.ExpireSolution(true);
        }
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "ReverseTimeline", _toggle, true, _invert);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "input Timeline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Global_T", "T", "Global time", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Active", "Act", "a boolean value shows that the T is in the timeline", GH_ParamAccess.item);
        }
        private Interval _timeline;
        private bool _state;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _timeline = new Interval();
            var t = 0.0;
            DA.GetData("Timeline", ref _timeline);
            DA.GetData("Global_T", ref t);
            var result = _timeline.IncludesParameter(t);
            
            result = _invert ? !result : result;
            _state = result;
            this.MessageSetup();
            DA.SetData("Active", result);
        }
        protected override string ShowTimeSetupDescription()
        {
            return TimelineSetting.TimelineDescription(_timeline) + $"\n status : {_state}";

        }
    }
}
using Grasshopper;
using Grasshopper.Kernel;
using System;
using Rhino;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;
using System.Windows.Forms;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Active timeline zone component. Inputs include ActivateInZone, InTimeline, OutTimeline, and Global_T. Outputs include Pointer_t.
    /// </summary>
    public class GH_ActiveTimelineZone : GH_TimelineAbstract
    {
        public GH_ActiveTimelineZone() : base("Active Timeline in Zone", "ATLZ", "Active timeline zone component") { }
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override Guid ComponentGuid => new Guid("d1c8e5b0-9a3c-4f1e-9b2a-8f7c6e5d4a3b");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Active_TL_In_Zone;
        private bool _reverseValue = false;
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Reverse Timeline", ReverseTime, true, _reverseValue);
        }
        protected virtual void ReverseTime(object sender, EventArgs e)
        {
            _reverseValue = !_reverseValue;
            ExpireSolution(true);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("ActivateInZone", "AZ", "Activate in zone", GH_ParamAccess.item, true);
            pManager.AddIntervalParameter("InTimeline", "ITL", "Input timeline", GH_ParamAccess.item);
            pManager.AddIntervalParameter("OutTimeline", "OTL", "Output timeline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Global_T", "T", "Global time", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Pointer_t", "t", "Pointer time ", GH_ParamAccess.item);
        }

        protected override string ShowTimeSetupDescription()
        => _isActive ? $"Active in zone, Global T: {_global_T}\n Input Timeline: {TimelineSetting.TimelineDescription(_iTL)}\n " +
        $" Output Timeline: {TimelineSetting.TimelineDescription(_oTL)} \n Pointer t: {_pointer_t}" :
        "Inactive in zone, pointer t = -1";
        private Interval _iTL = new Interval();
        private Interval _oTL = new Interval();
        private double _global_T = 0.0;
        private double _pointer_t = 0.0;
        private bool _isActive = false;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool activateInZone = false;
            var iTL = new Interval();
            var oTL = new Interval();
            var global_T = 0.0;
            var needExpired = false;
            DA.GetData("ActivateInZone", ref activateInZone);
            DA.GetData("InTimeline", ref iTL);
            needExpired = DA.GetData("OutTimeline", ref oTL);
            DA.GetData("Global_T", ref global_T);

            this._iTL = iTL;
            this._oTL = oTL;
            this._global_T = global_T;

            this._isActive = activateInZone;

            var t = needExpired ? TimelineSetting.ActivativeTimeline(activateInZone, iTL, oTL, global_T) : TimelineSetting.ActivativeTimeline(activateInZone, iTL, global_T);
            this._pointer_t = _reverseValue ? 1 - t : t;

            this.MessageSetup();

            DA.SetData("Pointer_t", _pointer_t);
        }
    }
}
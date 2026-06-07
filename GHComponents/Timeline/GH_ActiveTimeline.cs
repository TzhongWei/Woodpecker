using System;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Active timeline component. Inputs include InTimeline, OutTimeline, and Global_T. Outputs include Pointer_t.
    /// </summary>
    public class GH_ActiveTimeline : GH_TimelineAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public GH_ActiveTimeline() : base("Active Timeline", "ATL", "Active timeline component") { }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Active_TL;
        public override Guid ComponentGuid => new Guid("0727c17e-61b5-422e-999d-f22739a0b2d1");

        private bool _reverseValue = false;
        protected virtual void ReverseTime(object sender, EventArgs e)
        {
            _reverseValue = !_reverseValue;
            ExpireSolution(true);
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Reverse Timeline", ReverseTime, true, _reverseValue);
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("InTimeline", "ITL", "Input timeline", GH_ParamAccess.item);
            pManager.AddIntervalParameter("OutTimeline", "OTL", "Output timeline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Global_T", "T", "Global time", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Pointer_t", "t", "Pointer time ", GH_ParamAccess.item);
        }

        protected override string ShowTimeSetupDescription()
            => Message = $"Global T: {_global_T}\n Input Timeline: {TimelineSetting.TimelineDescription(_iTL)}\n " +
            $"Output Timeline: {TimelineSetting.TimelineDescription(_oTL)} \n Pointer t: {_pointer_t}";

        private Interval _iTL = new Interval();
        private Interval _oTL = new Interval();
        private double _global_T = 0.0;
        private double _pointer_t = 0.0;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var iTL = new Interval();
            var oTL = new Interval();
            var global_T = 0.0;
            var needExpired = false;
            if (!DA.GetData("InTimeline", ref iTL))
            {
                _pointer_t = 0.0;
                DA.SetData("Pointer_t", _pointer_t);
                return;
            }
            needExpired = DA.GetData("OutTimeline", ref oTL);
            if (!DA.GetData("Global_T", ref global_T))
            {
                _pointer_t = 0.0;
                DA.SetData("Pointer_t", _pointer_t);
                return;
            }
            this._iTL = iTL;
            this._oTL = oTL;
            this._global_T = global_T;
            this.MessageSetup();
            var t = needExpired ? TimelineSetting.ActivativeTimeline(iTL, oTL, global_T) : TimelineSetting.ActivativeTimeline(iTL, global_T);
            this._pointer_t = _reverseValue ? 1 - t : t;
            DA.SetData("Pointer_t", this._pointer_t);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ReverseValue", _reverseValue);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("ReverseValue"))
            {
                reader.TryGetBoolean("ReverseValue", ref _reverseValue);
            }
            return base.Read(reader);
        }
    }
}

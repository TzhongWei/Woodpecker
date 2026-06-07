using System;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_ActiveCircularTimeline : GH_TimelineAbstract
    {
        public GH_ActiveCircularTimeline() : base("Active circular timeline", "ACTL", "Evaluate whether a timeline value is active within a circular time range.")
        { }
        public override Guid ComponentGuid => new Guid("a9b72cd9-355b-4780-8816-92017f6b3f31");
        private bool _reverseValue = false;
        protected virtual void ReverseTime(object sender, EventArgs e)
        {
            _reverseValue = !_reverseValue;
            ExpireSolution(true);
        }
        private Interval _iTL = new Interval();
        private Interval _oTL = new Interval();
        private double _global_T = 0.0;
        private double _pointer_t = 0.0;
        protected override string ShowTimeSetupDescription()
    => Message = $"Global T: {_global_T}\n Input Timeline: {TimelineSetting.TimelineDescription(_iTL)}\n " +
    $"Output Timeline: {TimelineSetting.TimelineDescription(_oTL)} \n Pointer t: {_pointer_t}";
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("InTimeline", "ITL", "Input timeline", GH_ParamAccess.item);
            pManager.AddIntervalParameter("OutTimeline", "OTL", "Output timeline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Global_T", "T", "Global time", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Active_Circular_TL;

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Pointer_t", "t", "Pointer time ", GH_ParamAccess.item);
        }


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
            var t = needExpired ? TimelineSetting.ActivativeCircularTimeline(iTL, oTL, global_T) : TimelineSetting.ActivativeCircularTimeline(iTL, global_T);
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

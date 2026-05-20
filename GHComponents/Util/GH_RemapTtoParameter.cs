using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_RemapTtoParameter : GH_TimelineAbstract
    {
        public GH_RemapTtoParameter():base("Remap t to Parameter Range", "Remap_t", "")
        {
            this.SubCategory = "Util";
        }
        public override Guid ComponentGuid => new Guid("80c805af-4f62-453e-91f2-573633f38953");
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double t = 0.0;
            Interval range = new Interval();
            int digit = 1;
            DA.GetData("t", ref t);

            _t = Math.Max(0, Math.Min(1, t));

            DA.GetData("Parameter Range", ref range);
            _range = range;
            DA.GetData("Digits", ref digit);
            _value = TimelineSetting.RemapTtoSliderControl(t, range, digit);
            DA.SetData("Slider Value", _value);
        }
        private double _t;
        private Interval _range;
        private double _value;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("t", "t", "", GH_ParamAccess.item, 0);
            pManager.AddIntervalParameter("Parameter Range", "P Range", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Digits", "D", "", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Slider Value", "Value", "", GH_ParamAccess.item);
        }

        protected override string ShowTimeSetupDescription()
        {
            return $"Current t: {_t} \n " +
            $"Range: {TimelineSetting.TimelineDescription(_range)} \n " +
            $"Mapping Value: {_value}";
            
        }
    }
}
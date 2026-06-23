using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_RemapTtoParameter : GH_TimelineAbstract
    {
        public GH_RemapTtoParameter():base("Remap t to Parameter Range", "Remap_t", "Remap a normalised timeline value to a numeric parameter range.")
        {
            this.SubCategory = "Util";
        }
        public override Guid ComponentGuid => new Guid("80c805af-4f62-453e-91f2-573633f38953");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Remap_T;
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
            double defaultVal = -1;
            DA.GetData("DefaultValue", ref defaultVal);
            _value = TimelineSetting.RemapTtoSliderControl(t, defaultVal, range, digit);
            DA.SetData("Slider Value", _value);
        }
        private double _t;
        private Interval _range;
        private double _value;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("t", "t", "Normalised timeline value between 0 and 1.", GH_ParamAccess.item, 0);
            pManager.AddIntervalParameter("Parameter Range", "P Range", "Target numeric range for the remapped value.", GH_ParamAccess.item);
            pManager.AddNumberParameter("DefaultValue", "DVal", "The default parameter of the value", GH_ParamAccess.item);
            pManager[2].Optional = true;;
            pManager.AddIntegerParameter("Digits", "D", "Number of decimal digits used by the slider-style remap.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Slider Value", "Value", "Remapped value in the target parameter range.", GH_ParamAccess.item);
        }

        protected override string ShowTimeSetupDescription()
        {
            return $"Current t: {_t} \n " +
            $"Range: {TimelineSetting.TimelineDescription(_range)} \n " +
            $"Mapping Value: {_value}";
            
        }
    }
}

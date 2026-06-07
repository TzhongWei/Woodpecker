using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Applies an easing function to a normalized time value (0–1), allowing smooth animation transitions. Inputs include t. Outputs include t.
    /// </summary>
    public class GH_Easing: GH_Component
    {
        public GH_Easing():base("Easing", "E", "Applies an easing function to a normalized time value (0–1), allowing smooth animation transitions.", "Woodpecker", "Util")
        {}

        public override Guid ComponentGuid => new Guid("353ce233-806c-4c45-98c4-fecc4404b17a");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Easing;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("t", "t", "Normalized time parameter (0–1) to be remapped using an easing function.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("t", "t", "Eased time value after applying the default easing function.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var t = 0.0;
            DA.GetData("t", ref t);
            t = TimelineSetting.Easing(t); //Default easing
            DA.SetData("t", t);
        }
    }
}
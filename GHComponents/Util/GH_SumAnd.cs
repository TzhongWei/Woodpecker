using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_SumAnd : GH_Component
    {
        public GH_SumAnd() : base("Sum And", "SumAnd", "Sums or concatenates boolean values with and gates", "Woodpecker", "Util") { }
        public override Guid ComponentGuid => new Guid("e5c8b9e7-5a3c-4f0b-9c8e-2a1b6f3e4d5f");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Boolean Values", "B", "Boolean values to sum or concatenate", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Result", "R", "The result of summing or concatenating the input booleans.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<bool> values = new List<bool>();
            DA.GetDataList("Boolean Values", values);

            DA.SetData("Result", values.Aggregate((a, b) => a & b));
        }
    }
}
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_SumAnd : GH_Component
    {
        public GH_SumAnd() : base("Sum And", "SumAnd", "Sums or concatenates boolean values with and gates", "Woodpecker", "Util") { }
        public override Guid ComponentGuid => new Guid("526f598f-ea91-4db5-b76e-490640d834df");
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
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Sum_And;
    }
}
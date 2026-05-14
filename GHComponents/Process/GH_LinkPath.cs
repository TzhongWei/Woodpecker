using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_LinkPath : GH_PathEditAbstract
    {
        public GH_LinkPath() : base("Link Paths", "LP", "Connect consecutive path curves using a list of link patterns.")
        {

        }

        public override Guid ComponentGuid => new Guid("2b2d5e7b-3dda-44d6-a22e-9e64e89c3a3c");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Crvs", "Input curves to connect in sequence.", GH_ParamAccess.list);
            // Pattern list defining how each consecutive pair of curves is connected.
            // 0 = None (no connection), 1 = Down, 2 = Up.
            // Length should typically be curves.Count - 1.
            pManager.AddIntegerParameter("Patterns", "P", "Pattern list defining how each pair of curves connects (0=None, 1=Down, 2=Up).", GH_ParamAccess.list, new List<int> { 1, 0 });
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("FinishPaths", "FPs", "Connected path curves according to the selected return mode.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var crvs = new List<Curve>();
            var pat = new List<int>();

            DA.GetDataList("Curves", crvs);
            DA.GetDataList("Patterns", pat);
            ReturnCurveOption _return = ReturnCurveOption.JointCurve;

            switch (_option)
            {
                case 0:
                    _return = ReturnCurveOption.JointCurve;
                    break;
                case 1:
                    _return = ReturnCurveOption.SplitCurve;
                    break;
                default:
                    _return = ReturnCurveOption.OnlyAddedCurve;
                    break;
            }


            var agg = PathUtil.LinkPath(crvs, pat, _return);

            DA.SetDataList("FinishPaths", agg);
        }
    }
}

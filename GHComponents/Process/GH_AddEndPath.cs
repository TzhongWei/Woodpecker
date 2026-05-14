using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_AddEndPath : GH_PathEditAbstract
    {
        public GH_AddEndPath():base("Add End Path", "AddP", "Extend a curve from its start or end by distance and direction values.")
        {
            
        }

        public override Guid ComponentGuid => new Guid("65d7c35a-67eb-4f7f-a7d9-e4e76f9e2d08");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "CP", "Base curve to extend.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "Dist", "Extension distance values.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Direction", "Dir", "Extension direction vectors. If empty, the curve tangent direction is used.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Addend", "A", "If true, extend from the curve end; otherwise extend from the curve start.", GH_ParamAccess.item, false);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("NewCurve", "NCP", "Extended curve result according to the selected return mode.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            var dist = new List<double>();
            var dir = new List<Vector3d>();
            bool addEnd = false;

            DA.GetData("Curve", ref crv);
            DA.GetDataList("Distance", dist);
            DA.GetDataList("Direction", dir);
            DA.GetData("Addend", ref addEnd);

            ReturnCurveOption _return = ReturnCurveOption.JointCurve;

            switch(_option)
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

            var agg = PathUtil.AddEndedPath(crv, dist, dir, addEnd, _return);

            DA.SetDataList("NewCurve", agg);
        }
    }
}

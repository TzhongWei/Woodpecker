// public static Curve IterativeOffset(Curve curve, double Gap, double T = 0, bool Direction = false, Plane plane = new Plane())

using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Offset a curve with a distance until reaching the limitation or cannot offset. Inputs include Curve, Distance, Seam_t, Plane, and Direction, and related settings. Outputs include Curve.
    /// </summary>
    public class GH_IterativeOffset : GH_Component
    {
        public GH_IterativeOffset(): base("Iterative Offset", "IOffset", "Offset a curve with a distance until reaching the limitation or cannot offset", "Woodpecker", "Process")
        {
            
        }
        public override Guid ComponentGuid => new Guid("1d3acd68-9ee1-423b-848f-0cda01030a3d");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Crv", "The input closed curve for offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "Offset Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("Seam_t", "S_t", "The parameter of the seam", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "PL", "Plane for offset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Direction", "Dir", "The offset direction is either inward or outward", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Limit", "L", "The limitation for the offset", GH_ParamAccess.item, 10);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }
        
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Crv", "the output offset curves", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve Crv = null;
            double dist = 1.0;
            double seam_t = 0;
            Plane PL = Plane.WorldXY;
            bool dir = false;
            int limitation = 10;
            DA.GetData("Curve", ref Crv);
            DA.GetData("Distance", ref dist);
            DA.GetData("Limit", ref limitation);
            if(!DA.GetData("Seam_t", ref seam_t))
            {
                seam_t = 0;
            }
            if(!DA.GetData("Plane", ref PL))
            {
                PL = new Plane();
            }
            if(!DA.GetData("Direction", ref dir))
            {
                dir = false;
            }

            var oCrv = PathUtil.IterativeOffset(Crv, dist, seam_t, limitation, dir, PL);
            DA.SetData("Curve", oCrv);
        }
    }
}
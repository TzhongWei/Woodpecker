using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_PlaneToPlaneAction : GH_GeometryActionAbstract
    {
        public GH_PlaneToPlaneAction() : base("Plane to Plane Action", "P2PA", "") { }
        public override Guid ComponentGuid => new Guid("ebfa8089-0e5e-4b67-92f3-99d36c17aff0");

        protected override GeometryActionAbstract _geometryActionAbstract { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The name of this action", GH_ParamAccess.item, "Orientation");
            pManager.AddIntervalParameter("Timeline", "TL", "", GH_ParamAccess.item, new Interval(0, 1));
            pManager.AddPlaneParameter("PlaneA", "A", "", GH_ParamAccess.item);
            pManager.AddPlaneParameter("PlaneB", "B", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Action", "A", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            var timeline = new Interval();
            Plane pA = new Plane(), pB = new Plane();
            DA.GetData("Name", ref name);
            DA.GetData("Timeline", ref timeline);
            DA.GetData("PlaneA", ref pA);
            DA.GetData("PlaneB", ref pB);

            this._geometryActionAbstract = new GeometryFromPlaneToPlane(name, pA, pB, timeline.Min, timeline.Max);

            DA.SetData("Action", this._geometryActionAbstract);
        }
    }
}
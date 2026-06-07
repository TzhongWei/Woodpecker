using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates a timed orientation action between two planes. The source and target planes define the transformation, and the output is an action object for the geometry animation pipeline. Inputs include Name, Timeline, PlaneA, and PlaneB. Outputs include Action.
    /// </summary>
    public class GH_PlaneToPlaneAction : GH_GeometryActionAbstract
    {
        public GH_PlaneToPlaneAction() : base("Plane to Plane Action", "P2PA", "Create a timed plane-to-plane orientation action for the geometry animation pipeline.") { }
        public override Guid ComponentGuid => new Guid("ebfa8089-0e5e-4b67-92f3-99d36c17aff0");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Orient_Action;

        protected override GeometryActionAbstract _geometryActionAbstract { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The name of this action", GH_ParamAccess.item, "Orientation");
            pManager.AddIntervalParameter("Timeline", "TL", "Time interval in which this plane-to-plane action is evaluated.", GH_ParamAccess.item, new Interval(0, 1));
            pManager.AddPlaneParameter("PlaneA", "A", "Source plane for the orientation action.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("PlaneB", "B", "Target plane for the orientation action.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Action", "A", "Plane-to-plane action object for Geometry Transform Animation.", GH_ParamAccess.item);
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

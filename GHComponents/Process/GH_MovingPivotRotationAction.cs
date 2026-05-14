using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_MovingPivotRotationAction : GH_GeometryActionAbstract
    {
        public GH_MovingPivotRotationAction():base("Moving Pivot Rotation Action", "Rot Move", "Create a timed rotation action for the geometry animation pipeline."){}
        public override Guid ComponentGuid => new Guid("b783f75d-0c2d-4fc0-87b3-a4bf6c312559");
        protected override GeometryActionAbstract _geometryActionAbstract {get; set;}
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The name of this action", GH_ParamAccess.item, "Rotation");
            pManager.AddIntervalParameter("Timeline", "TL", "Time interval in which this rotation action is evaluated.", GH_ParamAccess.item, new Interval(0,1));
            pManager.AddVectorParameter("Axis", "X", "Rotation axis vector.", GH_ParamAccess.item);
            pManager.AddAngleParameter("Angle", "A", "Total rotation angle evaluated across the action timeline.", GH_ParamAccess.item, 0.5 * Math.PI);
            pManager.AddPointParameter("Centre", "C", "Rotation centre point.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Action", "A", "Rotation action object for Geometry Transform Animation.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pt = new Point3d();
            var angle = 0.0;
            var axis = new Vector3d();
            string name = "";
            var timeline = new Interval();
            DA.GetData("Name", ref name);
            DA.GetData("Timeline", ref timeline);
            DA.GetData("Axis", ref axis);
            DA.GetData("Angle", ref angle);
            DA.GetData("Centre", ref pt);
            this._geometryActionAbstract = new GeometryMovingPivotRotation(name, axis, angle, pt, timeline);
            DA.SetData("Action", this._geometryActionAbstract);
        }
    }
}

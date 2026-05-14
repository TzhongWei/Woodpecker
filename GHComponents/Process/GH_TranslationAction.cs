using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_TranslationAction : GH_GeometryActionAbstract
    {
        public GH_TranslationAction():base("Translation Action", "TA", "Create a timed translation action for the geometry animation pipeline."){}

        public override Guid ComponentGuid => new Guid("f82a470a-fa99-4818-9c75-4e2a2828e04f");

        protected override GeometryActionAbstract _geometryActionAbstract { get; set; }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The name of this action", GH_ParamAccess.item, "Translation");
            pManager.AddIntervalParameter("Timeline", "TL", "Time interval in which this translation action is evaluated.", GH_ParamAccess.item, new Interval(0,1));
            pManager.AddVectorParameter("MoveDirection", "Vec", "Translation direction vector.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "F", "Distance multiplier applied to the translation vector.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Action", "A", "Translation action object for Geometry Transform Animation.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            var timeline = new Interval();
            var vec = new Vector3d();
            var factor = 0.0;
            DA.GetData("Name", ref name);
            DA.GetData("Timeline", ref timeline);
            DA.GetData("MoveDirection", ref vec);
            DA.GetData("Factor", ref factor);
            this._geometryActionAbstract = new GeometryTranslation(name, vec, factor, timeline);
            DA.SetData("Action", this._geometryActionAbstract);
        }
    }
}

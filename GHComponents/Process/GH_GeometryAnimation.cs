using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Evaluates source geometry through ordered timed geometry actions. Geometry and action inputs are combined with a normalized pointer value, then the component outputs evaluated geometry and status messages. Inputs include Geometry, Pointer_t, and Actions. Outputs include Geometry and Message.
    /// </summary>
    public class GH_GeometryAnimation : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("6ac62e1f-8477-446c-80f2-fbde737503c6");

        public GH_GeometryAnimation(): base("Geometry Transform Animation", "GA", "Evaluate source geometry through an ordered list of timed geometry actions.", "Woodpecker", "Process"){

        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Tranform_Action;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Source geometry to animate.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "pointer t is a value between [0,1]", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actions", "As", "Timed geometry actions to apply in timeline order.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Animated geometry evaluated at the current pointer time.", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "M", "Status messages reported by the evaluated action pipeline.", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
           GeometryBase geom = null;
           double t = 0.0;
           var actionList = new List<GeometryActionAbstract>();
            DA.GetData("Geometry", ref geom);
            DA.GetData("Pointer_t", ref t);
            DA.GetDataList("Actions", actionList);

            var geomanimation = new GeometryAnimation(geom);
            geomanimation.AddRangeAction(actionList);

            var geomPipeline = new GeometryAnimationPipeline(geomanimation);
            geomPipeline.Animate(t);

            DA.SetData("Geometry", geomPipeline.GeomObject);
            DA.SetDataList("Message", geomPipeline.Message);
        }
    }
    [Obsolete]
    /// <summary>
    /// Legacy geometry animation evaluator. Inputs include Geometry, Pointer_t, and Actions. Outputs include Geometry and Message.
    /// </summary>
    public class GH_GeometryAnimation_Old : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override Guid ComponentGuid => new Guid("0be0a652-decd-46b6-88df-4060b3dae7ef");

        public GH_GeometryAnimation_Old(): base("Geometry Transform Animation", "GA", "Legacy geometry animation evaluator.", "Woodpecker", "Process"){

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Source geometry to animate.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Global_T", "T", "Global timeslot or special scope of timeslot from 0 to 1", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actions", "As", "Timed geometry actions to apply in timeline order.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Animated geometry evaluated at the current global time.", GH_ParamAccess.item);
            pManager.AddTextParameter("CMD", "CMD", "Status message from the last applied action.", GH_ParamAccess.item);
        }
        private GeometryAnimation _geometryAnimation;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geom = null;
            double t = 0.0;
            var actionList = new List<GeometryActionAbstract>();
            DA.GetData("Geometry", ref geom);
            DA.GetData("Global_T", ref t);
            DA.GetDataList("Actions", actionList);

            _geometryAnimation = new GeometryAnimation(geom);
            _geometryAnimation.AddRangeAction(actionList);
            var geom_result = _geometryAnimation.EvaluateGeometry_OLD(t);

            DA.SetData("Geometry", geom_result);
            DA.SetData("CMD", _geometryAnimation.Message);
        }
    }
}

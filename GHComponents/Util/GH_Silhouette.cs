
using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Extract silhouette outline curves from geometry for display or drafting. Inputs include Geometry. Outputs include Outline.
    /// </summary>
    public class GH_Silhouette: GH_Component
    {
        public override Guid ComponentGuid => new Guid("6d90bdf5-af93-4371-87a9-0d11d89ae8a6");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry from which to compute viewport silhouette outlines.", GH_ParamAccess.item);
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Silhouette;

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Outline", "OL", "Silhouette outline curves for the input geometry.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase G = null;
            DA.GetData("Geometry", ref G);
            var Crvs = DisplayUtil.DisplaySilhouette(G);

            DA.SetDataList("Outline", Crvs);
        }

        public GH_Silhouette():base("Silhouette", "Sil", "Extract silhouette outline curves from geometry for display or drafting.", "Woodpecker", "Util"){}
        
    }
}

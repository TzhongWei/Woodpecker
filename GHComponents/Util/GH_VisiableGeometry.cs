using System.Collections.Generic;
using System;
using Grasshopper.Kernel;
using Grasshopper;
using Woodpecker.Animation.Geometry.Display;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Filter out invisible geometries based on their indices. Inputs include Geometries and Invisible Indices. Outputs include Visible Geometries.
    /// </summary>
    public class GH_VisiableGeometry : GH_Component
    {
        public GH_VisiableGeometry() : base("Visible Geometry by indices", "VisGeo", "Filter out invisible geometries based on their indices.", "Woodpecker", "Util")
        {

        }

        public override Guid ComponentGuid => new Guid("d1c8e5b9-7c3a-4f0b-9c8e-2a1f5e6b7c8d");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Visiable_Geom;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "Geometry list to filter.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Invisible Indices", "I", "Indices of geometry items to remove from the visible output.", GH_ParamAccess.list);
            pManager[1].Optional = true; // Make the invisible indices input optional   
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Visible Geometries", "VG", "Geometry items whose indices are not marked invisible.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var geometries = new List<GeometryBase>();
            var invisibleIndices = new List<int>();

            DA.GetDataList("Geometries", geometries);
            DA.GetDataList("Invisible Indices", invisibleIndices);

            if(invisibleIndices.Count == 0)
            {
                for(int i = 0; i < invisibleIndices.Count; i += 2)
                {
                    invisibleIndices.Add(i);
                }
            }
            
            var visibleGeometries = DisplayUtil.VisibleGeometries(geometries, invisibleIndices);

            DA.SetDataList("Visible Geometries", visibleGeometries);
        }
    }
}

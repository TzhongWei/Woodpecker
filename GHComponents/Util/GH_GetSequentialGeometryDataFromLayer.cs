using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_GetSequentialGeometryDataFromLayer: GH_Component
    {
        public GH_GetSequentialGeometryDataFromLayer():base("Get Sequential Geometry Data From Layer", "SeqLayerGeo", "Get geometry from sequentially named layers and output matching animation indices.", "Woodpecker", "Util"){}
        public override Guid ComponentGuid => new Guid("ee05a2cd-0293-4cb8-b55b-d3f5c2e7f1d7");
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Refresh", Refresh);
        }
        private void Refresh()
        {
            this.ExpireSolution(true);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Layer Name Prefix", "FullName", "Full layer path prefix. Sequential indices are appended to this prefix.", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Range", "Range", "Optional layer index range. If omitted or empty, layers are scanned from 0 upward.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Indice", "Ind", "Animation sequence index for each output geometry.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Geometries", "Geoms", "Geometry collected from matching layers.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("GeomsTree", "GeoTree", "Geometry grouped by sequential layer order.", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
           string name = "";
           Interval range = new Interval();
            DA.GetData("Layer Name Prefix", ref name);
            DA.GetData("Range", ref range);

            if(!DataUtil.GetSequentialGeometryFromLayer(name, range, out var ind, out var geoms, out var geoTrees))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No matching layer geometry was found.");
            }

            DA.SetDataList("Indice", ind);
            DA.SetDataList("Geometries", geoms);
            DA.SetDataTree(2, geoTrees);
        } 
    }
}

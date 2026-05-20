using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System;
using Woodpecker.Animation.Util.IO;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates or updates a named geometry-code entry. A code name and geometry input are encoded into the geometry-code data structure for saving or downstream use. Inputs include CodeName and GeometryList. Outputs include GeometryCode.
    /// </summary>
    public class GH_CreateGeometry : GH_Component
    {
        public override GH_Exposure Exposure =>  GH_Exposure.tertiary;
        public GH_CreateGeometry()
          : base("Create GeometryCodes", "CGC",
              "Create a new geometry code",
              "Woodpecker", "GeometryCode")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CodeName", "CN", "The name of the geometry code to edit", GH_ParamAccess.item, "");
            pManager.AddGeometryParameter("GeometryList", "Gs", "List of Geometry objects for the geometry code", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GeometryCode", "GC", "Encoded geometry code", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var codeName = "";
            var geomList = new List<GeometryBase>();
            DA.GetData("CodeName", ref codeName);
            DA.GetDataList("GeometryList", geomList);
            var geomPairlist = geomList.SelectMany(x => (List<string>)(GeometryDataPair)x).ToList();
            var Code = new List<string>{codeName};
            Code.AddRange(geomPairlist);
            
            DA.SetDataList(0, Code);
        }
        public override Guid ComponentGuid => new Guid("0ab17d97-c1a6-468c-96d4-ae23b3632e18");
    }
}
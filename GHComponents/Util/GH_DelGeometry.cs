using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.DocObjects.Tables;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    [Obsolete]
    /// <summary>
    /// Delete geometry from the database. Inputs include SaveTrigger, GeometryPath, and GeometryName. Outputs include Deleted.
    /// </summary>
    public class GH_DelGeometry : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_DelGeometry():base("Deleted Geometry", "DG", "Delete geometry from the database", "Woodpecker", "Util"){}
        public override Guid ComponentGuid => new Guid("93af0d61-dccd-4d23-8c13-1c568e36fbec");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("SaveTrigger", "ST", "Trigger to save the Geometry to file", GH_ParamAccess.item, false);
            pManager.AddTextParameter("GeometryPath", "GP", "Path to the Geometry file. If -1 is provided, use default", GH_ParamAccess.item, "./data/GeometryData.json");
            pManager.AddTextParameter("GeometryName", "GN", "The name of the object need to save to the file", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Deleted", "D", "Whether the Geometry was successfully deleted", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var path = "";
            var Name = "";
            var saveTrigger = false;
            DA.GetData("SaveTrigger", ref saveTrigger);
            DA.GetData("GeometryPath", ref path);
            DA.GetData("GeometryName", ref Name);
            var result = false;
            if(saveTrigger)
            {
                result = GeometryCodeUtil.DeleteGeometryFromJson(path, Name);
            }
            DA.SetData("Deleted", result);

        }
    }
}
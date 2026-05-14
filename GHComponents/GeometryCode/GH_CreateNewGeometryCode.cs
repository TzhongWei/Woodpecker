using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using System.Linq;
using Woodpecker.Animation.Util.IO;
using System.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_CreateNewGeometryCodeFile : GH_Component
    {
        public GH_CreateNewGeometryCodeFile() : base("Create a New GeometryCode File", "NewGCodeBook", "Create a new geometry code file at the target directory.", "Woodpecker", "GeometryCode")
        {
            
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("9f0686ab-7f9b-4ddd-b288-96c064ba5df6");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Directory(), "Directory", "Dir", "The directory of the file that you want to create", GH_ParamAccess.item);
            pManager.AddTextParameter("FileName", "Name", "The name of the geometry file", GH_ParamAccess.item);
            pManager.AddTextParameter("GeometryCode", "GC", "Encoded geometry code to save", GH_ParamAccess.tree);
            pManager[2].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Saved", "S", "Whether the geometry code was successfully saved", GH_ParamAccess.item);
            pManager.AddTextParameter("NewPath", "path", "The new path of the geometrycode", GH_ParamAccess.item);
        }
        private bool _save = false;
        private string _filePath = "";
        private void _saveTrigger()
        {
            _save = GeometryCodeIO.CreateNewGeometryCode(_filePath, codeDic);
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Save", _saveTrigger, "create a new geometrycode");
        }
        private  Dictionary<string,  List<GeometryBase>> codeDic = new Dictionary<string,  List<GeometryBase>>();
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            codeDic = new Dictionary<string,  List<GeometryBase>>();
            var dir = "";
            var name = "";
            DA.GetData("Directory", ref dir);
            DA.GetData("FileName", ref name);

            _filePath = Path.Combine(dir, name);

            if (!System.Text.RegularExpressions.Regex.IsMatch(_filePath, @"\.json$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                _filePath += ".json";

            if (DA.GetDataTree<GH_String>("GeometryCode", out var geometryCodeTree) || geometryCodeTree == null)
            {
                var gcCode = GeometryCodeUtil.GetDefaultGeometryCode();
                codeDic = gcCode.GeomValues;
            }

            for (int i = 0; i < geometryCodeTree.Branches.Count; i++)
            {
                var branch = geometryCodeTree.Branches[i];
                if (branch.Count == 0) continue;

                var objectName = branch[0].Value;
                List<GeometryBase> geoms = new List<GeometryBase>();
                var dataList = branch.Select(x => x.Value).ToList();
                dataList.Remove(objectName);
                for (int j = 1; j < dataList.Count; j += 2)
                {
                    var geomJson = dataList[j];
                    GeometryBase geomObject = null;
                    try
                    {
                        geomObject = GeometryCodeUtil.DeserialiseGeometry(geomJson);
                    }
                    catch
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"The provide code is error at the branch {i}");
                    }
                    geoms.Add(geomObject);
                }
                codeDic[objectName] = geoms;
            }

            DA.SetData("Saved", _save);
            DA.SetData("NewPath", _filePath);
        }
    }
}

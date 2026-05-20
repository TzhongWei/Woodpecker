using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.UI.Theme;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Save geometry code to file. Inputs include GeometryCode. Outputs include Saved.
    /// </summary>
    public class GH_SaveGeometryCode : GH_GeometryCodeAbstract
    {
        public GH_SaveGeometryCode() : base("Save GeometryCodes", "SG", "Save geometry code to file") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public override Guid ComponentGuid => new Guid("3824b295-355f-4b00-966d-ac8fbbbbf5f9");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("GeometryCode", "GC", "Encoded geometry code", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Saved", "S", "Whether the geometry code was successfully saved", GH_ParamAccess.item);
        }
        private bool _overwrite = false;
        private void ToggleOverwrite(object sender, EventArgs e)
        {
            _overwrite = !_overwrite;
            ExpireSolution(true);
        }
        private Dictionary<string, List<GeometryBase>> codeDic;
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            Menu_AppendSeparator(menu);

            Menu_AppendItem(menu, "Overwrite", ToggleOverwrite, true, _overwrite);
        }
        private bool _saveResult = false;
        private void _saveToggle()
        {
            try
            {
                //ColourCodeUtil.CCParameters = new ColourCodeParameters(ColourCodeUtil.StringToColourDictionary(codeDic));
                ProjectAppManager.GCParameters = new GeometryCodeParameters(codeDic);
                _saveResult = GeometryCodeIO.SaveGeometryCode();
                var doc = this.OnPingDocument();
                if (doc == null) return;
                CodeManager.RefleshGHDocument.RefleshComponents(doc, this.UpdateTag);
            }
            catch (Exception ex)
            {

                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to save colour code: {ex.Message}");
                _saveResult = false;
            }
        }
        public override void CreateAttributes()
        {
            this.m_attributes = new ButtonUIAttributes(this, "Save", _saveToggle, "Save Colour Code");
        }
        private List<string> _comparedata = new List<string>();
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            codeDic = new Dictionary<string, List<GeometryBase>>();
            
            if(ProjectAppManager.GCParameters == null)
            {
                GeometryCodeIO.SetDefaultGeometryCode();
            }

            this.Message = _overwrite ? "Overwrite GC" : "Add GC";

            if (!DA.GetDataTree<GH_String>("GeometryCode", out var geometryCodeTree) || geometryCodeTree == null)
            {
                DA.SetData("Saved", false);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No colour code data provided.");
                return;
            }
            
            var newData = geometryCodeTree.Select(x => x.Value).ToList();
            if(!_comparedata.SequenceEqual(newData))
            {
                _saveResult = false;
                _comparedata = newData;
            }

            codeDic = _overwrite ? new Dictionary<string, List<GeometryBase>>() : ProjectAppManager.GCParameters.GeomValues;

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
            DA.SetData("Saved", _saveResult);
        }
    }

    [Obsolete]
    /// <summary>
    /// Save named Rhino geometries to a JSON file. Existing entries with the same name will be overwritten. Inputs include GeometryCode. Outputs include Saved.
    /// </summary>
    public class GH_SaveGeometry_Old : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_SaveGeometry_Old() : base("SaveGeometry", "SG", "Save named Rhino geometries to a JSON file. Existing entries with the same name will be overwritten.", "Woodpecker", "Util") { }
        public override Guid ComponentGuid => new Guid("f93133ce-d975-4fce-ad87-9db4b21e1b6a");
        private bool _overwrite = false;
        private void ToggleOverwrite(object sender, EventArgs e)
        {
            _overwrite = !_overwrite;
            ExpireSolution(true);
        }
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Overwrite", ToggleOverwrite, true, _overwrite);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("SaveTrigger", "ST", "Trigger to save the Geometry to file", GH_ParamAccess.item, false);
            pManager.AddTextParameter("GeometryPath", "GP", "Path to the Geometry file. If -1 is provided, use default", GH_ParamAccess.item, "./data/GeometryData.json");
            pManager.AddTextParameter("GeometryNames", "GNs", "The name of the object need to save to the file", GH_ParamAccess.list);
            pManager.AddGeometryParameter("SGeometries", "SGs", "Geometry need to save in the file", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Saved", "S", "Whether the Geometry was successfully saved", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool saveTrigger = false;
            string Path = "";
            var Names = new List<string>();
            var Geoms = new List<GeometryBase>();
            this.Message = _overwrite ? "Overwrite Geoms" : "Add Geoms";
            DA.GetData("SaveTrigger", ref saveTrigger);
            DA.GetData("GeometryPath", ref Path);
            DA.GetDataList("GeometryNames", Names);
            DA.GetDataList("SGeometries", Geoms);

            if (Names.Count != Geoms.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of geometry names must match the number of geometries.");
                DA.SetData("Saved", false);
            }
            if (Names.Count == 0 || Geoms.Count == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "At least one geometry name and one geometry must be provided before saving.");
                DA.SetData("Saved", false);
            }
            var GeomData = GeometryCodeUtil.CreateGeometryData(Names, Geoms);
            if (saveTrigger)
                DA.SetData("Saved", _overwrite ? GeometryCodeUtil.OverwriteGeometryToJson(Path, GeomData.ToArray()) : GeometryCodeUtil.WriteGeometryToJson(Path, GeomData.ToArray()));
            else
                DA.SetData("Saved", false);

        }
    }
}
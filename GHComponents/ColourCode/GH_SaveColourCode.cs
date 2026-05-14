using Grasshopper;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using System;
using Grasshopper.Kernel.Types;
using System.Windows.Forms;
using Woodpecker.Animation.Util.IO;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_SaveColourCode : GH_ColourCodeAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_SaveColourCode() : base("Save ColourCodes", "SCC",
              "Save the colour code to file")
        { }
        private bool _overwrite = false;
        private void ToggleOverwrite(object sender, EventArgs e)
        {
            _overwrite = !_overwrite;
            ExpireSolution(true);
        }
        private Dictionary<string, List<string>> codeDic;
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            Menu_AppendSeparator(menu);

            Menu_AppendItem(menu, "Overwrite", ToggleOverwrite, true, _overwrite);
        }
        private bool _saveResult = false;
        private List<string> _compareList = new List<string>();
        private void _saveToggle()
        {
            try
            {
                ProjectAppManager.CCParameters = new ColourCodeParameters(ColourCodeUtil.StringToColourDictionary(codeDic));
                _saveResult = ColourCodeIO.SaveColourCode();

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
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code to save", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Saved", "S", "Whether the colour code was successfully saved", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            codeDic = new Dictionary<string, List<string>>();
            
            if(ProjectAppManager.CCParameters == null)
            {
                ColourCodeIO.SetDefaultColourCode();
            }

            this.Message = _overwrite ? "Overwrite CC" : "Add CC";

            if (!DA.GetDataTree<GH_String>("ColourCode", out var colourCodeTree) || colourCodeTree == null)
            {
                DA.SetData("Saved", false);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No colour code data provided.");
                return;
            }
            var newList = colourCodeTree.Select(x => x.Value).ToList();
            if(!_compareList.SequenceEqual(newList))
            {
                _saveResult = false;
                _compareList = newList;
            }

            codeDic = _overwrite ?
             new Dictionary<string, List<string>>() :
            ColourCodeUtil.ColourDictionaryToString(ProjectAppManager.CCParameters.Values);
            for (int i = 0; i < colourCodeTree.Branches.Count; i++)
            {
                var branch = colourCodeTree.Branches[i];
                if (branch.Count == 0) continue;
                var key = branch[0].Value;

                //Check the format of the values, they should be in CSS color format (e.g., "rgb(255, 0, 0)" or "rgba(255, 0, 0, 0.5)")
                foreach (var Code in branch.Skip(1))
                {
                    var value = Code.Value.Trim().ToLowerInvariant();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^rgba?\s*\(\s*(\d{1,3}\s*,\s*){2}\d{1,3}(,\s*(0|1|0?\.\d+))?\s*\)$"))
                    {
                        DA.SetData("Saved", false);
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid color format for '{value}' in colour code '{key}'. Expected format: 'rgb(r, g, b)' or 'rgba(r, g, b, a)'.");
                        return;
                    }
                }

                var values = branch.Skip(1).Select(x => x.Value).ToList();
                codeDic[key] = values;
            }
            DA.SetData("Saved", _saveResult);

        }
        public override Guid ComponentGuid => new Guid("7acc291f-dfe2-4dc4-aea5-02e9ef781c36");
    }

    [Obsolete]
    public class GH_SaveColourCode_old : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_SaveColourCode_old()
          : base("SaveColourCode", "SCC",
              "Save the colour code to file",
              "Woodpecker", "ColourCode")
        {
        }
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
            pManager.AddBooleanParameter("SaveTrigger", "ST", "Trigger to save the colour code to file", GH_ParamAccess.item, false);
            pManager.AddTextParameter("ColourCodePath", "CP", "Path to save the colour code file", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code to save", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Saved", "S", "Whether the colour code was successfully saved", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool saveTrigger = false;
            string path = "";

            DA.GetData("SaveTrigger", ref saveTrigger);
            DA.GetData("ColourCodePath", ref path);
            this.Message = _overwrite ? "Overwrite CC" : "Add CC";
            if (!saveTrigger)
            {
                DA.SetData("Saved", false);
                return;
            }

            if (!DA.GetDataTree<GH_String>("ColourCode", out var colourCodeTree) || colourCodeTree == null)
            {
                DA.SetData("Saved", false);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No colour code data provided.");
                return;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                var dir = Path.Combine("./", "data");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                path = Path.Combine(dir, "ColourCode.json");
            }
            else
            {
                var parent = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
                    Directory.CreateDirectory(parent);
            }

            var codeDic = _overwrite ?
             new Dictionary<string, List<string>>() :
             ColourCodeUtil.ColourDictionaryToString(ColourCodeUtil.GetColourCode(path));
            for (int i = 0; i < colourCodeTree.Branches.Count; i++)
            {
                var branch = colourCodeTree.Branches[i];
                if (branch.Count == 0) continue;
                var key = branch[0].Value;

                //Check the format of the values, they should be in CSS color format (e.g., "rgb(255, 0, 0)" or "rgba(255, 0, 0, 0.5)")
                foreach (var Code in branch.Skip(1))
                {
                    var value = Code.Value.Trim().ToLowerInvariant();
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^rgba?\s*\(\s*(\d{1,3}\s*,\s*){2}\d{1,3}(,\s*(0|1|0?\.\d+))?\s*\)$"))
                    {
                        DA.SetData("Saved", false);
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid color format for '{value}' in colour code '{key}'. Expected format: 'rgb(r, g, b)' or 'rgba(r, g, b, a)'.");
                        return;
                    }
                }

                var values = branch.Skip(1).Select(x => x.Value).ToList();
                codeDic[key] = values;
            }

            if (saveTrigger)
                try
                {
                    DA.SetData("Saved", Woodpecker.Animation.Util.IO.ColourCodeUtil.SaveColourCode(path, codeDic));
                }
                catch (Exception ex)
                {
                    DA.SetData("Saved", false);
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to save colour code: {ex.Message}");
                }
        }
        public override Guid ComponentGuid => new Guid("32bceaa4-2072-4bc2-bc96-9a16511db0e8");
    }
}
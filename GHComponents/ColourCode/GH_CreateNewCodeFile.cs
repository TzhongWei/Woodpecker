using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using System.Linq;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using System.Drawing;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates a new colour-code file at a selected directory. Directory, file name, and optional colour-code data are combined into a saved JSON codebook path. Inputs include FileName and ColourCode. Outputs include Saved and NewPath.
    /// </summary>
    public class GH_CreateNewColourCodeFile : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_CreateNewColourCodeFile() : base("Create a New ColourCode File", "NewCCodeBook", "Create a new colour code file at the target directory.", "Woodpecker", "ColourCode")
        {

        }

        public override Guid ComponentGuid => new Guid("c6c682ed-1209-4520-a9fa-b8b365da4912");

        protected override Bitmap Icon => Properties.Resources.GH_Create_New_Colour_Code;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_Param_Directory(), "Directory", "Dir", "The directory of the file that you want to create", GH_ParamAccess.item);
            pManager.AddTextParameter("FileName", "Name", "The name of the colourcode file", GH_ParamAccess.item);
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code to save", GH_ParamAccess.tree);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Saved", "S", "Whether the colour code was successfully saved", GH_ParamAccess.item);
            pManager.AddTextParameter("NewPath", "path", "The new path of the colourcode", GH_ParamAccess.item);
        }
        private bool _saveresult = false;
        private string _filePath = "";
        private Dictionary<string, List<string>> codeDic;
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Save", _saveTrigger, "create a new colourcode");
        }
        public void _saveTrigger()
        {
            _saveresult = ColourCodeIO.CreateNewColourCode(_filePath, codeDic);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this.codeDic = new Dictionary<string, List<string>>();
            var dir = "";
            var name = "";
            DA.GetData("Directory", ref dir);
            DA.GetData("FileName", ref name);


            _filePath = Path.Combine(dir, name);

            if (!System.Text.RegularExpressions.Regex.IsMatch(_filePath, @"\.json$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                _filePath += ".json";

            if (DA.GetDataTree<GH_String>("ColourCode", out var colourCodeTree) || colourCodeTree == null)
            {
                var defaultCode = ColourCodeUtil.GetDefaultColourCode();
                this.codeDic = ColourCodeUtil.ColourDictionaryToString(defaultCode.Values);
            }
            else
            {
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
            }

            DA.SetData("Saved", _saveresult);
            DA.SetData("NewPath", _filePath);
        }
    }
}

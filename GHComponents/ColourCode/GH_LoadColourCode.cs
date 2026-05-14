using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Woodpecker.Animation.Util.IO;
using Grasshopper.Kernel.Data;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.CodeManager;
using System.Windows.Forms;
using Rhino.Render.CustomRenderMeshes;
using Eto.Forms;


namespace Woodpecker.Animation.GHComponents
{
    public class GH_LoadColourCode : GH_ColourCodeAbstract, ISingletonDocumentComponent, ISelectExistFile
    {
        public string SingletonTag => "LColourCode";
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public GH_LoadColourCode() : base("Load Colour", "CC",
              "Get existing Colour Code from the file data")
        { }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Select a Colour code json", Menu_SelectExistingFileClicked);
        }
        public void ShowEditor()
        {
            Select_SingleExistingFileClicked();
        }
        public void Menu_SelectExistingFileClicked(object sender, EventArgs e)
        {
            Select_SingleExistingFileClicked();
        }
        public bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return true;

            var sameComponents = doc.Objects
                .OfType<GH_Component>()
                .Where(x => x.GetType() == this.GetType())
                .OrderBy(x => x.InstanceGuid)
                .ToList();

            if (sameComponents.Count == 0) return true;

            return sameComponents.First().InstanceGuid == this.InstanceGuid;
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCodePath", "CP", "Path to the colour code file. If empty string is provided, use default", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code", GH_ParamAccess.tree);
        }
        public override Guid ComponentGuid => new Guid("d8c0a0b3-21ff-4bfa-bddd-fff6525bb3a9");
        protected override System.Drawing.Bitmap Icon => null;
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesEditable(this, "Load", Select_SingleExistingFileClicked, ShowEditor, "Load colour code");
        }
        public void After_Select_RefreshComponent()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return;
            CodeManager.RefleshGHDocument.RefleshComponents(doc, this.UpdateTag);
        }
        public void Select_SingleExistingFileClicked()
        {
            SelectExistFileExtensions.Select_SingleExistingFileClicked(this, "Select a colour code file");
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsPrimaryInstance())
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                 "Another Load Geometry component is already active in this document. This instance is locked.");
                 return;
            }

            var path = "";
            if (DA.GetData("ColourCodePath", ref path))
            {
                ColourCodeIO.ReadColourFromPath(path);
            }
            if(ProjectAppManager.CCParameters == null)
            {
                ColourCodeIO.SetDefaultColourCode();
            }
            DA.SetDataTree(0, ProjectAppManager.CCParameters.To_GH_DataTree());
        }
    }
    [Obsolete]
    public class GH_ColourCode_Old : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_ColourCode_Old()
          : base("ColourCode", "CC",
              "Get existing Colour Code from the file data",
              "Woodpecker", "ColourCode")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCodePath", "CP", "Path to the colour code file. If -1 is provided, use default", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            DA.GetData("ColourCodePath", ref path);
            if (path == "-1")
            {
                ColourCodeUtil.SetDefaultColourCode_Old();
                var dir = Path.Combine("./", "data");

                path = Path.Combine(dir, "ColourCode.json");
            }
            //If no path is provided, fall back to the default colour code file.
            else if (string.IsNullOrWhiteSpace(path))
            {
                ColourCodeUtil.SetDefaultColourCode("");
                var dir = Path.Combine("./", "data");

                path = Path.Combine(dir, "ColourCode.json");
            }
            else if (!File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"The specified colour code file does not exist at path: {path}. Falling back to default colour code.");
                Woodpecker.Animation.Util.IO.ColourCodeUtil.SetDefaultColourCode(path);
                var dir = Path.Combine("./", "data");

                path = Path.Combine(dir, "ColourCode.json");
            }
            var code = JsonRead.Load<Dictionary<string, List<string>>>(path);
            var NewTree = new DataTree<string>();
            for (int i = 0; i < code.Count; i++)
            {
                var key = code.Keys.ElementAt(i);
                var value = code[key];
                NewTree.Add(key, new GH_Path(i));
                NewTree.AddRange(value, new GH_Path(i));
            }
            DA.SetDataTree(0, NewTree);
        }
        public override Guid ComponentGuid => new Guid("D1B9C8F2-5E3B-4F7A-9C1A-2B3E4F5A6B7C");
        protected override System.Drawing.Bitmap Icon => null;
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Reflesh", FunctionToRunOnClick, "Undate colour code");
        }
        public void FunctionToRunOnClick()
        {
            this.ExpireSolution(true);
        }
    }
}

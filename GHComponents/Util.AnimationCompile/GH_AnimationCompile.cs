using System;
using System.Net.Mail;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.AnimationCompile;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Opens and stores animation compile settings, then runs the frame-to-movie compile process. It uses folder, naming, frame, and overwrite settings to produce a movie output. Inputs include Animation Setting. Outputs include Result.
    /// </summary>
    public class GH_AnimationCompile : GH_Component, IEditableWindow
    {
        public GH_AnimationCompile():base("Animation Compile", "Com", "Compile rendered animation frames into a .mov file.", "Woodpecker", "Util"){}
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public override Guid ComponentGuid => new Guid("833260b6-13f0-47fa-8008-7b9e0af6a90a");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Animation Setting", "Setting", "JSON animation compile setting.", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Result", "Compile result from the animation exporter.", GH_ParamAccess.item);
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesEditable(this, "Compile", run, ShowEditor);
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Open Setting Window", AnimationSettingWindow, true, false);
        }
        public void ShowEditor()
        {
            var window = new AnimationCompileWindow(setting ?? AnimationSetting.Default);
            var result = window.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (!result || window.Setting == null)
                return;

            setting = window.Setting;
            var settingJson = setting.ToJson();

            RecordUndoEvent("Animation Compile Setting");

            if (Params.Input[0] is Param_String input && input.SourceCount == 0)
            {
                _inputJson = settingJson;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The Setting input is connected, so the window value cannot overwrite the input data.");
            }

            ExpireSolution(true);
        }
        private void AnimationSettingWindow(object sender, EventArgs e)
        {
            ShowEditor();
        }
        private bool result = false;
        private void run()
        {
            result = AnimationCompileUtil.Compile(setting);
            this.ExpireSolution(false);
        }
        private AnimationSetting setting;
        private string _inputJson = "";
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var settingJson = "";
            DA.GetData("Animation Setting", ref settingJson);

            if(_inputJson == "" && settingJson != _inputJson)
            {
                _inputJson = settingJson;
            }

            if(settingJson == "" && _inputJson == "")
            {
                settingJson = AnimationSetting.Default.ToJson();
            }
            else
            {
                settingJson = _inputJson;
            }
            
            if(!AnimationSetting.FromJson(settingJson, out setting))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Setting file is an error format");
                DA.SetData("Result", "Error Setting format. Json parsing failed");
                return;
            }
            DA.SetData("Result", result.ToString());
        }
        public override bool Write(GH_IWriter writer)
        {
            if(_inputJson != "")
                writer.SetString("Setting", _inputJson);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("Setting", ref _inputJson);
            return base.Read(reader);
        }
    }
}

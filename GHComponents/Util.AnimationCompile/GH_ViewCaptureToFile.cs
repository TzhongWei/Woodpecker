using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Control.Camera;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.AnimationCompile;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Captures the active viewport or an optional camera parameter to an image file using a render setting.
    /// </summary>
    public class GH_ViewCaptureToFile : GH_Component, IEditableWindow
    {
        public GH_ViewCaptureToFile() : base("View Capture To File", "VCTF", "Capture the active viewport or a camera view to an image file.", "Woodpecker", "Util")
        { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("df52d404-9f4d-4465-b163-c61bdc56a123");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new GH_CameraParam(), "Camera", "Cam", "Optional camera parameter used for the capture. If empty, the active viewport is captured.", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddTextParameter("Render Setting", "Setting", "JSON render setting.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesEditable(this, "Render", run, ShowEditor);
        }
        public void ShowEditor()
        {
            var window = new RenderSettingWindow(this._parameter, _setting ?? RenderSetting.Default);
            var result = window.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (!result || window.Setting == null)
                return;
            _setting = window.Setting;
            var settingJson = _setting.ToJson();
            RecordUndoEvent("Render Setting");

            if (Params.Input[1] is Param_String input && input.SourceCount == 0)
            {
                _inputJson = settingJson;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The Setting input is connected, so the window value cannot overwrite the input data.");
            }

            ExpireSolution(true);
        }
        private string _inputJson = "";
        private RenderSetting _setting;
        private void run()
        {
            var renderSetting = _setting ?? RenderSetting.Default;
            using (Bitmap bitmap = CaptureBitmap(renderSetting))
            {
                if (bitmap == null)
                {
                    result = false;
                    ExpireSolution(false);
                    return;
                }

                result = RenderUtil.SaveBitmap(bitmap, renderSetting, RenderUtil.GetFrameName(renderSetting, 1));
            }

            ExpireSolution(false);
        }

        private Bitmap CaptureBitmap(RenderSetting setting)
        {
            Bitmap bitmap;
            if (_parameter == null)
                result = RenderUtil.Render_A_Image(setting, out bitmap);
            else
                result = RenderUtil.Render_A_Image_From_Camera(_parameter, setting, out bitmap);

            if (result)
                return bitmap;

            bitmap?.Dispose();
            return null;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Result", "view capture result", GH_ParamAccess.item);
        }
        private CameraParameter _parameter = null;
        private bool result = false;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string settingJson = "";
            DA.GetData("Render Setting", ref settingJson);

            GH_CameraGoo cameraGoo = null;
            DA.GetData("Camera", ref cameraGoo);
            _parameter = cameraGoo?.CameraValue;

            if (string.IsNullOrWhiteSpace(_inputJson) && settingJson != _inputJson)
                _inputJson = settingJson;
            if (string.IsNullOrWhiteSpace(settingJson) && string.IsNullOrWhiteSpace(_inputJson))
                settingJson = RenderSetting.Default.ToJson();
            else
                settingJson = _inputJson;

            if (!RenderSetting.FromJson(settingJson, out _setting))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Setting file is an error format");
                DA.SetData("Result", "Error Setting format. Json parsing failed");
                return;
            }
            DA.SetData("Result", result.ToString());
        }
    }
}

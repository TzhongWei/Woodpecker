using System;
using Grasshopper.Kernel;
using Rhino.Commands;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.AnimationCompile;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Mirrors rendered animation frame images in a folder. The folder and mirror direction control a batch image-processing operation for animation frames. Inputs include Animation Setting. Outputs include Result.
    /// </summary>
    public class GH_MirrorFrames : GH_Component
    {
        public GH_MirrorFrames() : base("Mirror Animation frames", "Mirror Frames", "Mirror animation frame images horizontally or vertically.", "Woodpecker", "Util") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("b32d18f5-63ee-4f3a-ae41-4180ce109e4a");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Animation Setting", "Setting", "JSON animation compile setting.", GH_ParamAccess.item, AnimationSetting.Default.ToJson());
            pManager.AddBooleanParameter("Horizontal", "H", "Mirror frames horizontally when true; otherwise mirror vertically.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Result", "Compile result from the animation exporter.", GH_ParamAccess.item);

        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributes(this, "Mirror", run);
        }
        private bool result = true;
        private bool hor = false;
        private AnimationSetting setting;
        private void run()
        {
            result = AnimationCompileUtil.MirrorFrames(setting, hor);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var settingJson = "";
            DA.GetData("Animation Setting", ref settingJson);
            DA.GetData("Horizontal", ref hor);
            if (!AnimationSetting.FromJson(settingJson, out setting))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Setting file is an error format");
                DA.SetData("Result", "Error Setting format. Json parsing failed");
                return;
            }
            DA.SetData("Result", result.ToString());
        }
    }
}

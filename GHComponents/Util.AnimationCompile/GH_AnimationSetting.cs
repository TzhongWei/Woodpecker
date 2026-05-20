using System;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Woodpecker.Animation.Util.AnimationCompile;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates a serialized animation compile setting from input/output folders, output name, frame filename pattern, frame duration, and overwrite behavior. Inputs include Input Folder, Output Folder, Output Name, Frame Prefix, and Frame Digits, and related settings. Outputs include Animation Setting.
    /// </summary>
    public class GH_AnimationSetting : GH_Component
    {
        public override Guid ComponentGuid => new Guid("6db48ee1-84c4-4718-ad65-d3a7b681e202");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_AnimationSetting():base("Animation Setting", "ASetting", "Create a JSON animation compile setting from frame and output options.", "Woodpecker", "Util"){}

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input Folder", "In_Folder", "Animation Frames locations", GH_ParamAccess.item);
            pManager.AddTextParameter("Output Folder", "Out_Folder", "Animation (.mov) file location", GH_ParamAccess.item);
            pManager.AddTextParameter("Output Name", "Name", "Output movie file name.", GH_ParamAccess.item, "Result.mov");
            pManager.AddTextParameter("Frame Prefix", "Prefix", "Prefix used by input frame filenames.", GH_ParamAccess.item, "Frame_");
            pManager.AddIntegerParameter("Frame Digits", "Digits", "Number of digits used to pad frame indices.", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Frame Duration", "Duration", "Duration of each frame in seconds.", GH_ParamAccess.item, 1.0 / 30.0);
            pManager.AddBooleanParameter("Overwrite", "Overwrite", "Whether to overwrite an existing output movie.", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Animation Setting", "Setting", "Serialized animation compile setting.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            var inF = "";
            var outF = "";
            var name = "";
            var pre = "";
            int dig = 0;
            double dur = 0;
            bool overwrite = true;
            DA.GetData(0, ref inF);
            DA.GetData(1, ref outF);
            DA.GetData(2, ref name);
            DA.GetData(3, ref pre);
            DA.GetData(4, ref dig);
            DA.GetData(5, ref dur);
            DA.GetData(6, ref overwrite);
            var setting = new AnimationSetting()
            {
                InputFolder = inF,
                OutputFolder = outF,
                OutputName = name,
                FramePrefix = pre,
                FrameDigits = dig,
                FrameDuration = dur,
                Overwrite = overwrite
            };
            var settingJson = JsonConvert.SerializeObject(setting);
            DA.SetData("Animation Setting", settingJson);
        }
    }
}

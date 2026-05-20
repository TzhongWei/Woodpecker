using System;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.AnimationCompile;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates a serialized render setting for viewport image capture.
    /// </summary>
    public class GH_RenderSetting : GH_Component
    {
        private RenderSetting _setting = RenderSetting.Default;

        public GH_RenderSetting()
            : base("Render Setting", "RSetting", "Create a JSON render setting for viewport image capture.", "Woodpecker", "Util")
        {
        }

        public override Guid ComponentGuid => new Guid("9d850d9d-fb1c-4e0e-a30e-7f69de8346ff");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Output Path", "Path", "Folder where rendered image frames are saved.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Size Width", "W", "Rendered image width in pixels.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Size Height", "H", "Rendered image height in pixels.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Mirrow Horizontal", "MH", "Mirror the rendered bitmap horizontally before saving.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Mirrow Vertical", "MV", "Mirror the rendered bitmap vertically before saving.", GH_ParamAccess.item);
            pManager.AddTextParameter("Frame Prefix", "Prefix", "Prefix used by rendered frame filenames.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Frame Digits", "Digits", "Number of digits used to pad frame indices.", GH_ParamAccess.item);
            pManager.AddTextParameter("Frame Extension", "Ext", "Rendered image file extension.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Overwrite", "Overwrite", "Whether rendering may overwrite existing frame files.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Transparent Background", "Transparent", "Capture the viewport with a transparent background when supported.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Draw Grid", "Grid", "Draw the construction grid in the viewport capture.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Draw Axes", "Axes", "Draw the construction plane axes in the viewport capture.", GH_ParamAccess.item);

            for (int i = 0; i < 12; i++)
                pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Render Setting", "Setting", "Serialized render setting.", GH_ParamAccess.item);
            pManager.AddTextParameter("Frame Name", "Frame", "Example frame filename produced by the render setting.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var setting = (_setting ?? RenderSetting.Default).Clone();

            var outputPath = setting.OutputPath;
            var sizeW = setting.SizeW;
            var sizeH = setting.SizeH;
            var mirrowH = setting.MirrowH;
            var mirrowV = setting.MirrowV;
            var framePrefix = setting.FramePrefix;
            var frameDigits = setting.FrameDigits;
            var frameExtension = setting.FrameExtension;
            var overwrite = setting.Overwrite;
            var transparentBackground = setting.TransparentBackground;
            var drawGrid = setting.DrawGrid;
            var drawAxes = setting.DrawAxes;

            DA.GetData(0, ref outputPath);
            DA.GetData(1, ref sizeW);
            DA.GetData(2, ref sizeH);
            DA.GetData(3, ref mirrowH);
            DA.GetData(4, ref mirrowV);
            DA.GetData(5, ref framePrefix);
            DA.GetData(6, ref frameDigits);
            DA.GetData(7, ref frameExtension);
            DA.GetData(8, ref overwrite);
            DA.GetData(9, ref transparentBackground);
            DA.GetData(10, ref drawGrid);
            DA.GetData(11, ref drawAxes);

            setting.OutputPath = outputPath ?? string.Empty;
            setting.SizeW = Math.Max(24, sizeW);
            setting.SizeH = Math.Max(24, sizeH);
            setting.MirrowH = mirrowH;
            setting.MirrowV = mirrowV;
            setting.FramePrefix = framePrefix ?? string.Empty;
            setting.FrameDigits = Math.Max(1, frameDigits);
            setting.FrameExtension = frameExtension;
            setting.Overwrite = overwrite;
            setting.TransparentBackground = transparentBackground;
            setting.DrawGrid = drawGrid;
            setting.DrawAxes = drawAxes;

            _setting = setting.Clone();

            DA.SetData("Render Setting", setting.ToJson());
            DA.SetData("Frame Name", GetFrameName(setting, 1));
        }

        private static string GetFrameName(RenderSetting setting, int frame)
        {
            var frameNumber = frame.ToString().PadLeft(setting.FrameDigits, '0');
            return $"{setting.FramePrefix}{frameNumber}.{setting.FrameExtension}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.AnimationCompile;

namespace Woodpecker.Animation.GHComponents
{
    internal enum RenderControllerMode
    {
        DirectTime,
        TaggedTime,
    }

    /// <summary>
    /// Controls automatic viewport rendering from a direct Global_T value or a tagged Global_T channel.
    /// </summary>
    public class GH_RenderController : GH_TimeSlotChannel_OUT, IEditableWindow
    {
        private RenderControllerMode _mode;
        private readonly List<Color> optionColours = new List<Color>
        {
            Color.FromArgb(70, 255, 81, 81),
            Color.FromArgb(70, 220, 255, 81)
        };

        private bool renderTrigger = false;
        private bool succ = false;
        private int _frameIndex = -1;
        private RenderSetting _setting;
        private string _inputJson = "";
        private Interval _renderRange = new Interval();

        public GH_RenderController() : base("Animation Render", "ARen", "Render animation frames from Global_T.", "Util")
        {
            _mode = RenderControllerMode.DirectTime;
            Message = "OFF";
        }

        public override Guid ComponentGuid => new Guid("7d2572b6-e3c0-4ed9-8314-be903ae1de70");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override RemoteType ChannelType => RemoteType.Output;

        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesStateEditable(this, new List<string>
            {
                "Render Off",
                "Render On"
            }, ToggleRenderTrigger, ShowEditor, optionColours, "Render Component State", 0, () => renderTrigger);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(
                menu,
                "Direct Global_T",
                (s, e) => SetMode(RenderControllerMode.DirectTime),
                true,
                _mode == RenderControllerMode.DirectTime);

            Menu_AppendItem(
                menu,
                "Tag Global_T",
                (s, e) => SetMode(RenderControllerMode.TaggedTime),
                true,
                _mode == RenderControllerMode.TaggedTime);
        }

        private void SetMode(RenderControllerMode mode)
        {
            if (_mode == mode) return;

            RecordUndoEvent("Change Render Controller Mode");
            _mode = mode;

            Params.UnregisterInputParameter(Params.Input[0], true);
            Params.RegisterInputParam(CreateTimeInputParam(), 0);

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        private IGH_Param CreateTimeInputParam()
        {
            if (_mode == RenderControllerMode.DirectTime)
            {
                return new Param_Number
                {
                    Name = "Global_T",
                    NickName = "T",
                    Description = "Frame time or frame index used to trigger rendering.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
            }

            return new Param_String
            {
                Name = "Tag",
                NickName = "Tag",
                Description = "Tag of the Global_T channel.",
                Access = GH_ParamAccess.item,
                Optional = true,
            };
        }

        private void ToggleRenderTrigger()
        {
            renderTrigger = !renderTrigger;
            Message = renderTrigger ? "RENDER ON" : "OFF";
            if (this.Attributes is ButtonUIAttributesState stateAttributes)
                stateAttributes.UpdateSelectedIndex(renderTrigger ? 1 : 0);
            this.Attributes?.ExpireLayout();
            this.OnDisplayExpired(true);
            ExpireSolution(true);
        }

        public void ShowEditor()
        {
            var window = new RenderSettingWindow(_setting ?? RenderSetting.Default);
            var result = window.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (!result || window.Setting == null)
                return;

            _setting = window.Setting;
            var settingJson = _setting.ToJson();
            RecordUndoEvent("Render Setting");

            if (Params.Input[2] is Param_String input && input.SourceCount == 0)
            {
                _inputJson = settingJson;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The Setting input is connected, so the window value cannot overwrite the input data.");
            }

            ExpireSolution(true);
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(CreateTimeInputParam());
            pManager.AddIntervalParameter("Render Range", "Frame Ran", "Optional frame range to render.", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Render Setting", "Setting", "JSON render setting.", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddIntegerParameter("Frame Multiplier", "Mul", "Multiplier used to convert Global_T to an integer frame index.", GH_ParamAccess.item, 2);
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "Result", "Render controller status.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!TryGetRenderSetting(DA, out _setting))
            {
                DA.SetData("Result", "Error Setting format. Json parsing failed");
                return;
            }

            var frameMultiplier = 1;
            DA.GetData("Frame Multiplier", ref frameMultiplier);
            frameMultiplier = Math.Max(1, frameMultiplier);

            var hasRenderRange = TryGetRenderRange(DA);

            if (!TryGetFrame(DA, frameMultiplier, out _frameIndex, out var frameMessage))
            {
                DA.SetData("Result", frameMessage);
                return;
            }

            var inRange = !hasRenderRange || _renderRange.IncludesParameter(_frameIndex);
            var message = BuildStatusMessage(hasRenderRange, inRange);

            if (renderTrigger && inRange)
            {
                if (TryRenderCurrentFrame(out var renderMessage))
                {
                    message += $"\n{renderMessage}";
                    message += $"Image save to : \n {Path.Combine(_setting.OutputPath, RenderUtil.GetFrameName(_setting, _frameIndex))}";
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, renderMessage);
                    message += $"\n{renderMessage}";
                }
            }
            else if (renderTrigger && !inRange)
            {
                message += "\nFrame is outside render range.";
            }

            DA.SetData("Result", message);
        }

        private bool TryGetRenderSetting(IGH_DataAccess DA, out RenderSetting setting)
        {
            var settingJson = "";
            DA.GetData("Render Setting", ref settingJson);

            if (!string.IsNullOrWhiteSpace(settingJson))
                _inputJson = settingJson;
            else if (!string.IsNullOrWhiteSpace(_inputJson))
                settingJson = _inputJson;
            else
                settingJson = RenderSetting.Default.ToJson();

            if (RenderSetting.FromJson(settingJson, out setting))
                return true;

            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Setting file is an error format.");
            return false;
        }

        private bool TryGetRenderRange(IGH_DataAccess DA)
        {
            var range = new Interval();
            if (!DA.GetData("Render Range", ref range))
                return false;

            _renderRange = range;
            return true;
        }

        private bool TryGetFrame(IGH_DataAccess DA, int frameMultiplier, out int frameIndex, out string message)
        {
            frameIndex = -1;
            message = string.Empty;

            if (!TryGetGlobalT(DA, out var frame))
            {
                message = "ERROR";
                return false;
            }
            frame = Math.Round(frame, 5);
            if (frame < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Frame index cannot be negative.");
                message = "Frame index cannot be negative.";
                return false;
            }

            var scaledFrame = frame * Math.Pow(10, frameMultiplier);
            var roundedFrame = Math.Round(scaledFrame);
            if (Math.Abs(roundedFrame - scaledFrame) > 1e-9)
            {
                message = $"Output frame name error => {frame} * {frameMultiplier} = {scaledFrame}, should be integer.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                return false;
            }

            frameIndex = (int)roundedFrame;
            return true;
        }

        private bool TryGetGlobalT(IGH_DataAccess DA, out double frame)
        {
            frame = 0.0;
            if (_mode == RenderControllerMode.DirectTime)
            {
                SingletonTag = string.Empty;
                return DA.GetData(0, ref frame);
            }

            var tag = "";
            if (!DA.GetData(0, ref tag) || string.IsNullOrWhiteSpace(tag))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tag cannot be null or white space.");
                return false;
            }

            SingletonTag = tag;
            if (!IsPrimaryInstance() || !GetValue(out var tValue))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Global T value must be a number or the tag {SingletonTag} cannot be found.");
                return false;
            }

            frame = tValue;
            return true;
        }

        private bool TryRenderCurrentFrame(out string message)
        {
            message = string.Empty;
            using (var bitmap = CaptureBitmap(_setting))
            {
                if (bitmap == null)
                {
                    succ = false;
                    message = "Render failed.";
                    return false;
                }

                var frameName = RenderUtil.GetFrameName(_setting, _frameIndex);
                succ = RenderUtil.SaveBitmap(bitmap, _setting, frameName);
                message = succ ? $"Rendered {frameName}" : $"Render failed: {frameName}";
                return succ;
            }
        }

        private Bitmap CaptureBitmap(RenderSetting setting)
        {
            if (!renderTrigger || setting == null)
                return null;

            var captured = RenderUtil.Render_A_Image(setting, out var bitmap);
            if (captured)
                return bitmap;

            bitmap?.Dispose();
            return null;
        }

        private string BuildStatusMessage(bool hasRenderRange, bool inRange)
        {
            var message = hasRenderRange
                ? $"Render range from {_renderRange.Min} to {_renderRange.Max}"
                : "Render all frames";

            message += $"\nCurrent frame is {RenderUtil.GetFrameName(_setting, _frameIndex)}";
            message += renderTrigger ? "\nRender trigger is on" : "\nRender trigger is off";
            if (!inRange)
                message += "\nFrame is outside render range";

            return message;
        }

        public override bool Write(GH_IWriter writer)
        {
            var writeMode = _mode == RenderControllerMode.DirectTime ? "DirectTime" : "TaggedTime";

            writer.SetString("MODE", writeMode);
            if(_inputJson != "")
                writer.SetString("Setting", _inputJson);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("Setting", ref _inputJson);
            string writeMode = "DirectTime";
            reader.TryGetString("MODE", ref writeMode);

            _mode = writeMode == "DirectTime" ? RenderControllerMode.DirectTime : RenderControllerMode.TaggedTime;

            return base.Read(reader);
        }
    
    }
}

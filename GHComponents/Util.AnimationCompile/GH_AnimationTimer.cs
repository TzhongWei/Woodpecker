using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Expressions;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.AnimationCompile;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_AnimationTimer : GH_TimeSlotChannel_Abstract, IEditableWindow, IGH_StateAwareObject, IGH_InitCodeAware
    {
        // Timer Setting
        private DateTime m_schedule;
        private enum InitNotation
        {
            None,
            Range,
            DoubleDot
        }
        private GH_SliderBase m_slider;
        private string m_expression;
        private bool _playState;
        private bool _tickScheduled;
        private bool _frameInProgress;
        private bool _remoteUpdateScheduled;
        private bool _waitingForSolutionEnd;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Global_Slider;
        public GH_SliderBase Slider
        {
            get
            {
                EnsureSlider();
                return m_slider;
            }
        }
        public decimal Step { get; set; } = 1m;
        public int IntervalMs { get; set; } = 30;
        public AnimationTimerSchedule ScheduleSetting { get; set; } = AnimationTimerSchedule._20ms;

        public string Expression
        {
            get => m_expression;
            set => m_expression = string.IsNullOrWhiteSpace(value) ? null : value;
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            Menu_AppendIntervalItems(menu, true);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Open Setting Window", SliderSettingWindow, true, false);
        }
        private void SliderSettingWindow(object sender, EventArgs e)
        {
            this.ShowEditor();
        }
        private void Menu_AppendIntervalItems(ToolStripDropDown menu, bool asSubMenu)
        {
            if(asSubMenu)
            {
                var toolStripMenuItem = GH_DocumentObject.Menu_AppendItem(menu, "Interval");
                toolStripMenuItem.ToolTipText = "Specify the delay between cyclic updates. ";
                menu = toolStripMenuItem.DropDown;
            }
            var toolStripMenuItem1 = GH_DocumentObject.Menu_AppendItem(menu, "10 ms", Menu_IntervalPreset_Clicked, true, IntervalMs == 10);
            var toolStripMenuItem2 = GH_DocumentObject.Menu_AppendItem(menu, "20 ms", Menu_IntervalPreset_Clicked, true, IntervalMs == 20);
            var toolStripMenuItem3 = GH_DocumentObject.Menu_AppendItem(menu, "50 ms", Menu_IntervalPreset_Clicked, true, IntervalMs == 50);
            var toolStripMenuItem4 = GH_DocumentObject.Menu_AppendItem(menu, "100 ms", Menu_IntervalPreset_Clicked, true, IntervalMs == 100);
            var toolStripMenuItem5 = GH_DocumentObject.Menu_AppendItem(menu, "200 ms", Menu_IntervalPreset_Clicked, true, IntervalMs == 200);
            var toolStripMenuItem6 = GH_DocumentObject.Menu_AppendItem(menu, "500 ms", Menu_IntervalPreset_Clicked, true, IntervalMs == 500);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            var toolStripMenuItem7 = GH_DocumentObject.Menu_AppendItem(menu, "1 s", Menu_IntervalPreset_Clicked, true, IntervalMs == 1000);
            var toolStripMenuItem8 = GH_DocumentObject.Menu_AppendItem(menu, "2 s", Menu_IntervalPreset_Clicked, true, IntervalMs == 2000);
            var toolStripMenuItem9 = GH_DocumentObject.Menu_AppendItem(menu, "5 s", Menu_IntervalPreset_Clicked, true, IntervalMs == 5000);
            var toolStripMenuItem10 = GH_DocumentObject.Menu_AppendItem(menu, "10 s", Menu_IntervalPreset_Clicked, true, IntervalMs == 10000);
            var toolStripMenuItem11 = GH_DocumentObject.Menu_AppendItem(menu, "20 s", Menu_IntervalPreset_Clicked, true, IntervalMs == 20000);
            var toolStripMenuItem12 = GH_DocumentObject.Menu_AppendItem(menu, "30 s", Menu_IntervalPreset_Clicked, true, IntervalMs == 30000);
            GH_DocumentObject.Menu_AppendSeparator(menu);
            var toolStripMenuItem13 = GH_DocumentObject.Menu_AppendItem(menu, "1 min", Menu_IntervalPreset_Clicked, true, IntervalMs == 60000);
            var toolStripMenuItem14 = GH_DocumentObject.Menu_AppendItem(menu, "5 min", Menu_IntervalPreset_Clicked, true, IntervalMs == 300000);
            var toolStripMenuItem15 = GH_DocumentObject.Menu_AppendItem(menu, "15 min", Menu_IntervalPreset_Clicked, true, IntervalMs == 900000);
            var toolStripMenuItem16 = GH_DocumentObject.Menu_AppendItem(menu, "1 hr", Menu_IntervalPreset_Clicked, true, IntervalMs == 3600000);
            toolStripMenuItem1.Tag = 10;
            toolStripMenuItem2.Tag = 20;
            toolStripMenuItem3.Tag = 50;
            toolStripMenuItem4.Tag = 100;
            toolStripMenuItem5.Tag = 200;
            toolStripMenuItem6.Tag = 500;
            toolStripMenuItem7.Tag = 1000;
            toolStripMenuItem8.Tag = 2000;
            toolStripMenuItem9.Tag = 5000;
            toolStripMenuItem10.Tag = 10000;
            toolStripMenuItem11.Tag = 20000;
            toolStripMenuItem12.Tag = 30000;
            toolStripMenuItem13.Tag = 60000;
            toolStripMenuItem14.Tag = 300000;
            toolStripMenuItem15.Tag = 900000;
            toolStripMenuItem16.Tag = 3600000;
            GH_DocumentObject.Menu_AppendSeparator(menu);
        }
        private void Menu_IntervalPreset_Clicked(object sender, EventArgs e)
        {
            var toolStripMenuItem = (ToolStripMenuItem)sender;
            IntervalMs = (int)toolStripMenuItem.Tag;
            ScheduleSetting = AnimationTimerScheduleUtil.FromMilliseconds(IntervalMs);
            if (PlayState)
                ScheduleNextTick();
        }
        public bool PlayState
        {
            get => _playState;
            set
            {
                if (_playState == value) return;

                _playState = value;
                Message = _playState ? "PLAY" : "STOP";
                OnDisplayExpired(false);

                if (_playState)
                    ScheduleNextTick();
                else
                {
                    m_schedule = DateTime.MinValue;
                    _tickScheduled = false;
                    _frameInProgress = false;
                    DetachFrameSolutionEnd();
                }
            }
        }

        public bool IsExpression => !string.IsNullOrWhiteSpace(m_expression);
        public override Guid ComponentGuid => new Guid("7c34a708-d546-4b25-95c8-1c490774609a");
        public override RemoteType ChannelType => RemoteType.Input;

        public GH_AnimationTimer() : base("Animation Timer", "AT", "Animation timer parameter", "Util")
        {
            EnsureSlider();
            Message = "STOP";
        }
        private void EnsureSlider()
        {
            if (m_slider != null) return;

            m_slider = new GH_SliderBase
            {
                Type = GH_SliderAccuracy.Float,
                Minimum = 0m,
                Maximum = 20m,
                Value = 0.00m,
                DecimalPlaces = 2,
                GripDisplay = GH_SliderGripDisplay.ShapeAndText,
                Font = GH_FontServer.StandardAdjusted,
                DrawControlBorder = false,
                DrawControlShadows = false,
                DrawControlBackground = false,
                TickCount = 11,
                TickFrequency = 5,
                RailDarkColour = Color.FromArgb(40, Color.Black),
                TickDisplay = GH_SliderTickDisplay.Simple,
                RailDisplay = GH_SliderRailDisplay.Simple,
                Padding = new Padding(6, 2, 6, 1)
            };
            m_slider.ValueChanged += InternalSliderValueChanged;
        }

        public override void CreateAttributes()
        {
            m_attributes = new AnimationTimerAttributes(this);
        }

        public override string InstanceDescription
        {
            get
            {
                var list = new List<string>();
                var minimum = Slider.Minimum;
                var maximum = Slider.Maximum;
                var value = Slider.Value;
                var percentage = maximum == minimum
                    ? 0m
                    : decimal.Multiply(100m, decimal.Divide(decimal.Subtract(value, minimum), decimal.Subtract(maximum, minimum)));

                switch (Slider.Type)
                {
                    case GH_SliderAccuracy.Float:
                        list.Add("Floating point accuracy");
                        break;
                    case GH_SliderAccuracy.Integer:
                        list.Add("Integer accuracy");
                        break;
                    case GH_SliderAccuracy.Even:
                        list.Add("Even number accuracy");
                        break;
                    case GH_SliderAccuracy.Odd:
                        list.Add("Odd number accuracy");
                        break;
                }

                list.Add($"Lower limit: {FormatNumber(minimum, Slider.Type, Slider.DecimalPlaces)}");
                list.Add($"Upper limit: {FormatNumber(maximum, Slider.Type, Slider.DecimalPlaces)}");
                list.Add($"Value: {FormatNumber(value, Slider.Type, Slider.DecimalPlaces)}");
                list.Add($"Factor: {percentage:0}%");
                list.Add($"Step: {Step}");
                list.Add($"Interval: {IntervalMs} ms");
                return string.Join(Environment.NewLine, list);
            }
        }

        private void InternalSliderValueChanged(object sender, GH_SliderEventArgs e)
        {
            ExpireSolution(true);
        }

        public bool TrySetSliderValue(decimal target)
        {
            if (!IsExpression)
            {
                Slider.Value = target;
                Slider.FixValue();
                return true;
            }

            if (!GH_Convert.BackSolveExpression(
                Expression,
                "x",
                Convert.ToDouble(target),
                Convert.ToDouble(Slider.Minimum),
                Convert.ToDouble(Slider.Maximum),
                10,
                out var t,
                out _))
            {
                return false;
            }

            Slider.Value = Convert.ToDecimal(t);
            Slider.FixValue();
            return true;
        }

        public void SetSliderValue(decimal value, bool recompute)
        {
            value = Math.Max(Slider.Minimum, Math.Min(Slider.Maximum, value));
            if (Slider.Value == value) return;

            Slider.RaiseEvents = false;
            Slider.Value = value;
            Slider.FixValue();
            Slider.RaiseEvents = true;
            ExpireSolution(recompute);
        }

        public void TogglePlay()
        {
            PlayState = !PlayState;
        }

        public void Play()
        {
            PlayState = true;
        }

        public void Stop()
        {
            PlayState = false;
        }

        private void ScheduleNextTick()
        {
            if (_tickScheduled || _frameInProgress || !PlayState) return;

            var doc = OnPingDocument();
            if (doc == null || !doc.Enabled) return;

            var interval = Math.Max(1, IntervalMs);
            var utcNow = DateTime.UtcNow;

            _tickScheduled = true;
            if (DateTime.Compare(m_schedule, utcNow) > 0)
            {
                var remaining = Convert.ToInt32((m_schedule - utcNow).TotalMilliseconds);
                doc.ScheduleSolution(Math.Max(1, remaining), ScheduleCallBack);
                return;
            }

            m_schedule = utcNow + TimeSpan.FromMilliseconds(interval);
            doc.ScheduleSolution(interval, ScheduleCallBack);
        }

        private void ScheduleCallBack(GH_Document doc)
        {
            _tickScheduled = false;
            if (!PlayState)
            {
                m_schedule = DateTime.MinValue;
                return;
            }

            if (_frameInProgress)
                return;

            if (DateTime.Compare(DateTime.UtcNow, m_schedule - TimeSpan.FromMilliseconds(5.0)) < 0)
            {
                ScheduleNextTick();
                return;
            }

            m_schedule = DateTime.MinValue;
            _frameInProgress = true;
            AttachFrameSolutionEnd(doc);
            AdvanceOneStep();
            ExpireSolution(false);
        }

        private void AttachFrameSolutionEnd(GH_Document doc)
        {
            if (doc == null || _waitingForSolutionEnd) return;

            doc.SolutionEnd += OnFrameSolutionEnd;
            _waitingForSolutionEnd = true;
        }

        private void DetachFrameSolutionEnd()
        {
            var doc = OnPingDocument();
            if (doc == null || !_waitingForSolutionEnd) return;

            doc.SolutionEnd -= OnFrameSolutionEnd;
            _waitingForSolutionEnd = false;
        }

        private void OnFrameSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            if (sender is GH_Document doc)
                doc.SolutionEnd -= OnFrameSolutionEnd;

            _waitingForSolutionEnd = false;
            _frameInProgress = false;

            if (PlayState)
                ScheduleNextTick();
        }

        private void AdvanceOneStep()
        {
            var next = Slider.Value + Step;

            if (Step >= 0m && next >= Slider.Maximum)
            {
                next = Slider.Maximum;
                PlayState = false;
            }
            else if (Step < 0m && next <= Slider.Minimum)
            {
                next = Slider.Minimum;
                PlayState = false;
            }

            SetSliderValue(next, false);
        }

        public void ShowEditor()
        {
            using (var form = new AnimationTimerWindows(this))
            {
                GH_WindowsFormUtil.CenterFormOnCursor(form, true);
                if (form.ShowDialog(Grasshopper.Instances.DocumentEditor) != System.Windows.Forms.DialogResult.OK)
                    return;

                RecordUndoEvent("Animation timer setting");
                form.ApplyToOwner();

                if (Params.Input[0] is Param_String input && input.SourceCount == 0)
                {
                    input.PersistentData.Clear();
                    input.PersistentData.Append(new GH_String(SingletonTag));
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The Tag input is connected, so the window tag cannot overwrite the input data.");
                }

                Attributes?.ExpireLayout();
                ExpireSolution(true);
            }
        }
        public void SetTag(string tag)
        {
            tag = string.IsNullOrWhiteSpace(tag) ? "Global_T" : tag.Trim();
            _tagFromWindows = tag;
            SingletonTag = tag;
            ExpireSolution(true);
        }
        private string _tagFromWindows = "";
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "Time slot tag used to publish and identify the animation timer value.", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Global T", "T", "Current animation timer value.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var tag = string.Empty;
            var tagInput = Params.Input[0] as Param_String;
            var hasTagSource = tagInput != null && tagInput.SourceCount > 0;

            if (hasTagSource)
            {
                DA.GetData("Tag", ref tag);
            }
            else
            {
                tag = !string.IsNullOrWhiteSpace(_tagFromWindows) ? _tagFromWindows : SingletonTag;

                if (string.IsNullOrWhiteSpace(tag))
                    DA.GetData("Tag", ref tag);
            }

            if (string.IsNullOrWhiteSpace(tag))
                tag = "Global_T";

            _tagFromWindows = tag;

            var timeslotChannel = new TimeSlotTagChannel(tag);
            timeslotChannel.SetValue(Convert.ToDouble(Slider.Value));
            tagChannel = timeslotChannel;
            SingletonTag = tag;
            UpdateRemoteOutput();

            DA.SetData("Global T", Convert.ToDouble(Slider.Value));
        }

        public override bool IsPrimaryInstance()
        {
            var doc = OnPingDocument();
            if (doc == null) return false;

            var sameTag = doc.Objects
                .OfType<GH_TagChannel_Abstract>()
                .Where(x => x.ChannelType == ChannelType)
                .Where(x => x.SingletonTag == SingletonTag)
                .OrderBy(x => x.InstanceGuid)
                .ToList();

            return sameTag.Count == 1;
        }

        private bool UpdateRemoteOutput()
        {
            var doc = OnPingDocument();
            if (doc == null || _remoteUpdateScheduled) return false;

            _remoteUpdateScheduled = true;
            doc.ScheduleSolution(1, d =>
            {
                _remoteUpdateScheduled = false;

                foreach (var outComp in d.Objects.OfType<GH_TagChannel_Abstract>()
                    .Where(x => x.ChannelType == RemoteType.Output && x.SingletonTag == SingletonTag))
                {
                    outComp.ExpireSolution(false);
                }
            });
            return true;
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetDouble("Value", Convert.ToDouble(Slider.Value));
            writer.SetDouble("Min", Convert.ToDouble(Slider.Minimum));
            writer.SetDouble("Max", Convert.ToDouble(Slider.Maximum));
            writer.SetInt32("Digits", Slider.DecimalPlaces);
            writer.SetDouble("Step", Convert.ToDouble(Step));
            writer.SetInt32("IntervalMs", IntervalMs);
            writer.SetInt32("Schedule", (int)ScheduleSetting);

            if (!string.IsNullOrWhiteSpace(m_expression))
                writer.SetString("Expression", m_expression);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (!base.Read(reader))
                return false;

            var value = 0.0;
            var min = 0.0;
            var max = 200.0;
            var digits = 3;
            var step = Convert.ToDouble(Step);
            var interval = IntervalMs;
            var schedule = (int)ScheduleSetting;

            reader.TryGetDouble("Value", ref value);
            reader.TryGetDouble("Min", ref min);
            reader.TryGetDouble("Max", ref max);
            reader.TryGetInt32("Digits", ref digits);
            reader.TryGetDouble("Step", ref step);
            reader.TryGetInt32("IntervalMs", ref interval);
            reader.TryGetInt32("Schedule", ref schedule);
            reader.TryGetString("Expression", ref m_expression);

            Slider.DecimalPlaces = digits;
            Slider.Minimum = Convert.ToDecimal(min);
            Slider.Maximum = Convert.ToDecimal(max);
            Slider.Value = Convert.ToDecimal(value);
            Step = Convert.ToDecimal(step);
            IntervalMs = Math.Max(1, interval);
            ScheduleSetting = Enum.IsDefined(typeof(AnimationTimerSchedule), schedule)
                ? (AnimationTimerSchedule)schedule
                : AnimationTimerScheduleUtil.FromMilliseconds(IntervalMs);

            Slider.FixDomain();
            Slider.FixValue();
            PlayState = false;

            return true;
        }

        public string SaveState()
        {
            var chunk = new GH_LooseChunk("AnimationSlider");
            chunk.SetDouble("Minumum", Convert.ToDouble(Slider.Minimum));
            chunk.SetDouble("Maximum", Convert.ToDouble(Slider.Maximum));
            chunk.SetDouble("Value", Convert.ToDouble(Slider.Value));
            chunk.SetInt32("Digits", Slider.DecimalPlaces);
            chunk.SetDouble("Step", Convert.ToDouble(Step));
            chunk.SetInt32("IntervalMs", IntervalMs);
            chunk.SetInt32("Schedule", (int)ScheduleSetting);
            return chunk.Serialize_Xml();
        }

        string IGH_StateAwareObject.SaveState() => SaveState();

        public void LoadState(string state)
        {
            var chunk = new GH_LooseChunk("AnimationSlider");
            chunk.Deserialize_Xml(state);

            var value = Convert.ToDouble(Slider.Minimum);
            var max = Convert.ToDouble(Slider.Maximum);
            var min = Convert.ToDouble(Slider.Minimum);
            var digits = Slider.DecimalPlaces;
            var step = Convert.ToDouble(Step);
            var interval = IntervalMs;
            var schedule = (int)ScheduleSetting;

            chunk.TryGetDouble("Value", ref value);
            chunk.TryGetDouble("Maximum", ref max);
            chunk.TryGetDouble("Minumum", ref min);
            chunk.TryGetInt32("Digits", ref digits);
            chunk.TryGetDouble("Step", ref step);
            chunk.TryGetInt32("IntervalMs", ref interval);
            chunk.TryGetInt32("Schedule", ref schedule);

            Slider.RaiseEvents = false;
            Slider.Minimum = Convert.ToDecimal(min);
            Slider.Maximum = Convert.ToDecimal(max);
            Slider.Value = Convert.ToDecimal(value);
            Slider.DecimalPlaces = digits;
            Slider.RaiseEvents = true;
            Step = Convert.ToDecimal(step);
            IntervalMs = Math.Max(1, interval);
            ScheduleSetting = Enum.IsDefined(typeof(AnimationTimerSchedule), schedule)
                ? (AnimationTimerSchedule)schedule
                : AnimationTimerScheduleUtil.FromMilliseconds(IntervalMs);
            PlayState = false;
            ExpireSolution(true);
        }

        void IGH_StateAwareObject.LoadState(string state) => LoadState(state);

        public void SetInitCode(string code)
        {
            PlayState = false;
            if (string.IsNullOrWhiteSpace(code)) return;

            code = code.Trim();
            try
            {
                if (HarvestRange(code, out var minText, out var maxText, out var valueText))
                {
                    if (GH_Convert.ToDouble(minText, out var min, GH_Conversion.Secondary) &&
                        GH_Convert.ToDouble(maxText, out var max, GH_Conversion.Secondary) &&
                        GH_Convert.ToDouble(valueText, out var value, GH_Conversion.Secondary))
                    {
                        Slider.Type = GH_SliderAccuracy.Float;
                        Slider.Minimum = Convert.ToDecimal(min);
                        Slider.Maximum = Convert.ToDecimal(max);
                        Slider.Value = Convert.ToDecimal(value);
                        Slider.DecimalPlaces = Math.Max(HarvestDecimalPlaces(minText), Math.Max(HarvestDecimalPlaces(maxText), HarvestDecimalPlaces(valueText)));
                        Slider.FixDomain();
                        Slider.FixValue();
                        ExpireSolution(true);
                        return;
                    }
                }

                var variant = GH_Convert.ParseExpression(code, true);
                switch (variant.Type)
                {
                    case GH_VariantType.@int:
                        Slider.Type = GH_SliderAccuracy.Integer;
                        Slider.Value = variant._Int;
                        break;
                    case GH_VariantType.@double:
                        Slider.Type = GH_SliderAccuracy.Float;
                        Slider.Value = Convert.ToDecimal(variant._Double);
                        Slider.DecimalPlaces = HarvestDecimalPlaces(code);
                        break;
                }

                Slider.FixDomain();
                Slider.FixValue();
                ExpireSolution(true);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to set init code: {ex.Message}");
            }
        }

        public static string FormatNumber(decimal value, GH_SliderAccuracy accuracy, int digits)
        {
            if (accuracy == GH_SliderAccuracy.Float)
                return string.Format("{0:#0." + new string('0', Math.Max(0, digits)) + "}", value);

            return $"{value:#0}";
        }

        public static int HarvestDecimalPlaces(string text)
        {
            text = text.Trim();
            if (text.Length == 0) return 0;

            var hasDecimal = false;
            var count = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (i == 0 && (c == '-' || c == '+')) continue;
                if (c == '.')
                {
                    if (hasDecimal) return 0;
                    hasDecimal = true;
                    continue;
                }

                if (!char.IsDigit(c)) return 0;
                if (hasDecimal) count++;
            }

            return Math.Min(count, 16);
        }

        public static bool HarvestRange(string text, out string minimum, out string maximum, out string value)
        {
            minimum = string.Empty;
            maximum = string.Empty;
            value = string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var parts = new[] { string.Empty, string.Empty, string.Empty };
            var partIndex = 0;
            var notation = InitNotation.None;
            var quoted = false;
            var bracketDepth = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '"')
                {
                    quoted = !quoted;
                    parts[partIndex] += c;
                    continue;
                }

                if (quoted)
                {
                    parts[partIndex] += c;
                    continue;
                }

                switch (c)
                {
                    case '(':
                    case '[':
                    case '{':
                        bracketDepth++;
                        parts[partIndex] += c;
                        continue;
                    case ')':
                    case ']':
                    case '}':
                        bracketDepth--;
                        if (bracketDepth < 0) return false;
                        parts[partIndex] += c;
                        continue;
                }

                if (bracketDepth > 0)
                {
                    parts[partIndex] += c;
                    continue;
                }

                if (c == '<')
                {
                    if (notation == InitNotation.DoubleDot) return false;
                    notation = InitNotation.Range;
                    partIndex++;
                    if (partIndex > 2) return false;
                    continue;
                }

                if (c == '.')
                {
                    var j = i + 1;
                    while (j < text.Length && text[j] == '.') j++;
                    if (j - i > 1)
                    {
                        if (notation == InitNotation.Range) return false;
                        notation = InitNotation.DoubleDot;
                        partIndex++;
                        if (partIndex > 2) return false;
                        i = j - 1;
                        continue;
                    }
                }

                parts[partIndex] += c;
            }

            if (notation == InitNotation.None) return false;
            if (string.IsNullOrWhiteSpace(parts[0])) return false;
            if (string.IsNullOrWhiteSpace(parts[partIndex])) partIndex--;
            if (partIndex < 0) return false;

            for (var i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i])) continue;
                var variant = GH_Convert.ParseExpression(parts[i], true);
                if (variant.Type != GH_VariantType.@double && variant.Type != GH_VariantType.@int)
                    return false;
            }

            switch (partIndex)
            {
                case 0:
                    if (GH_Convert.ToDouble(parts[0], out var destination, GH_Conversion.Secondary))
                    {
                        minimum = parts[0].Trim();
                        if (destination == 0.0)
                            maximum = "1.0";
                        else if (destination < 0.0)
                            maximum = GH_Convert.ToPrevPowerOfTen(destination).ToString();
                        else
                            maximum = GH_Convert.ToNextPowerOfTen(destination).ToString();

                        value = parts[0].Trim();
                    }
                    break;
                case 1:
                    minimum = parts[0].Trim();
                    maximum = parts[1].Trim();
                    value = parts[0].Trim();
                    break;
                case 2:
                    minimum = parts[0].Trim();
                    maximum = parts[2].Trim();
                    value = parts[1].Trim();
                    break;
            }

            return true;
        }
    }
}

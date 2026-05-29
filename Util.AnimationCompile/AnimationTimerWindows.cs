using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using Grasshopper.Kernel.Special;
using Woodpecker.Animation.GHComponents;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    public enum AnimationTimerSchedule
    {
        _10ms,
        _20ms,
        _50ms,
        _100ms,
        _200ms,
        _500ms,
        _1s,
        _2s,
        _5s,
        _10s,
        _20s,
        _30s,
        _1min,
        _5min,
        _15min,
        _1hr
    }

    public static class AnimationTimerScheduleUtil
    {
        public static int ToMilliseconds(AnimationTimerSchedule schedule)
        {
            switch (schedule)
            {
                case AnimationTimerSchedule._10ms: return 10;
                case AnimationTimerSchedule._20ms: return 20;
                case AnimationTimerSchedule._50ms: return 50;
                case AnimationTimerSchedule._100ms: return 100;
                case AnimationTimerSchedule._200ms: return 200;
                case AnimationTimerSchedule._500ms: return 500;
                case AnimationTimerSchedule._1s: return 1000;
                case AnimationTimerSchedule._2s: return 2000;
                case AnimationTimerSchedule._5s: return 5000;
                case AnimationTimerSchedule._10s: return 10000;
                case AnimationTimerSchedule._20s: return 20000;
                case AnimationTimerSchedule._30s: return 30000;
                case AnimationTimerSchedule._1min: return 60000;
                case AnimationTimerSchedule._5min: return 300000;
                case AnimationTimerSchedule._15min: return 900000;
                case AnimationTimerSchedule._1hr: return 3600000;
                default: return 30;
            }
        }

        public static AnimationTimerSchedule FromMilliseconds(int milliseconds)
        {
            var closest = AnimationTimerSchedule._20ms;
            var closestDistance = int.MaxValue;

            foreach (AnimationTimerSchedule schedule in Enum.GetValues(typeof(AnimationTimerSchedule)))
            {
                var distance = Math.Abs(ToMilliseconds(schedule) - milliseconds);
                if (distance >= closestDistance) continue;

                closest = schedule;
                closestDistance = distance;
            }

            return closest;
        }

        public static string DisplayName(AnimationTimerSchedule schedule)
        {
            return schedule.ToString().TrimStart('_').Replace("ms", " ms").Replace("min", " min").Replace("hr", " hr");
        }
    }

    public class AnimationTimerWindows : Form
    {
        private readonly TextBox _tagName;
        private readonly ComboBox _scheduleBox;
        private readonly GH_Slider sldDigits;
        private readonly Grasshopper.GUI.GH_DigitScroller numLower;
        private readonly Grasshopper.GUI.GH_DigitScroller numUpper;
        private readonly Grasshopper.GUI.GH_DigitScroller numRange;
        private readonly Grasshopper.GUI.GH_DigitScroller numStep;
        private readonly Grasshopper.GUI.GH_DigitScroller numValue;
        private readonly Button _ok;
        private readonly Button _cancel;

        private GH_AnimationTimer _owner;
        private bool _updating;

        public string TagName => _tagName.Text;
        public AnimationTimerSchedule Schedule
        {
            get
            {
                if (_scheduleBox.SelectedItem is AnimationTimerSchedule schedule)
                    return schedule;

                if (_owner != null)
                    return _owner.ScheduleSetting;

                return AnimationTimerSchedule._20ms;
            }
        }
        public int Digits => Convert.ToInt32(Math.Round(Convert.ToDouble(sldDigits.Value)));
        public decimal Lower => numLower.Value;
        public decimal Upper => numUpper.Value;
        public decimal Range => numRange.Value;
        public decimal Step => numStep.Value;
        public decimal Value => numValue.Value;
        public AnimationTimerWindows(GH_AnimationTimer owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            Text = "Animation Timer";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(360, 332);

            _tagName = new TextBox { Location = new Point(18, 18), Width = 324 };
            _scheduleBox = new ComboBox
            {
                Location = new Point(120, 52),
                Width = 222,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (AnimationTimerSchedule schedule in Enum.GetValues(typeof(AnimationTimerSchedule)))
                _scheduleBox.Items.Add(schedule);
            _scheduleBox.Format += (sender, args) =>
            {
                if (args.ListItem is AnimationTimerSchedule schedule)
                    args.Value = AnimationTimerScheduleUtil.DisplayName(schedule);
            };

            sldDigits = new GH_Slider
            {
                Location = new Point(120, 84),
                Size = new Size(222, 24),
                Minimum = 0m,
                Maximum = 6m,
                Value = owner.Slider.DecimalPlaces,
                Type = GH_SliderAccuracy.Integer,
                DecimalPlaces = 0,
                TickCount = 7,
                TickFrequency = 1,
                RailDisplay = GH_SliderRailDisplay.Simple,
                TickDisplay = GH_SliderTickDisplay.Simple,
                GripDisplay = GH_SliderGripDisplay.ShapeAndText
            };

            numLower = CreateScroller(128);
            numUpper = CreateScroller(160);
            numRange = CreateScroller(192);
            numStep = CreateScroller(224);
            numValue = CreateScroller(256);

            _ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(176, 292), Width = 78 };
            _cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(264, 292), Width = 78 };

            Controls.AddRange(new System.Windows.Forms.Control[]
            {
                _tagName,
                Label("Schedule", 18, 56),
                _scheduleBox,
                Label("Digit setting", 18, 88),
                sldDigits,
                Separator(18, 118, 324),
                Label("Lower bound", 18, 132),
                numLower,
                Label("Upper bound", 18, 164),
                numUpper,
                Label("Range bound", 18, 196),
                numRange,
                Label("Step", 18, 228),
                numStep,
                Label("Value", 18, 260),
                numValue,
                _ok,
                _cancel
            });

            AcceptButton = _ok;
            CancelButton = _cancel;

            sldDigits.ValueChanged += (sender, args) => UpdateDigitSettings();
            numLower.ValueChanged += (sender, args) => ClampDomain();
            numUpper.ValueChanged += (sender, args) => ClampDomain();
            numRange.ValueChanged += (sender, args) => UpdateUpperFromRange();

            Setup(owner);
        }

        public void Setup(GH_AnimationTimer owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            _updating = true;
            try
            {
                _tagName.Text = string.IsNullOrWhiteSpace(owner.SingletonTag) ? "Global_T" : owner.SingletonTag;
                SelectSchedule(owner.ScheduleSetting);
                sldDigits.Value = owner.Slider.DecimalPlaces;
                UpdateDigitSettings();

                SetScroller(numLower, -100000000m, 100000000m, owner.Slider.Minimum);
                SetScroller(numUpper, -100000000m, 100000000m, owner.Slider.Maximum);
                SetScroller(numRange, 0m, 200000000m, Math.Max(0m, owner.Slider.Maximum - owner.Slider.Minimum));
                SetScroller(numStep, -100000000m, 100000000m, owner.Step);
                SetScroller(numValue, owner.Slider.Minimum, owner.Slider.Maximum, owner.Slider.Value);
                UpdateDigitSettings();
            }
            finally
            {
                _updating = false;
            }
        }

        public void ApplyToOwner()
        {
            if (_owner == null) return;

            var lower = Math.Min(Lower, Upper);
            var upper = Math.Max(Lower, Upper);
            var value = Math.Max(lower, Math.Min(upper, Value));
            var schedule = Schedule;

            _owner.SetTag(string.IsNullOrWhiteSpace(TagName) ? "Global_T" : TagName.Trim());
            _owner.ScheduleSetting = schedule;
            _owner.IntervalMs = AnimationTimerScheduleUtil.ToMilliseconds(schedule);
            _owner.Slider.DecimalPlaces = Math.Max(0, Math.Min(6, Digits));
            _owner.Slider.Minimum = lower;
            _owner.Slider.Maximum = upper;
            _owner.Slider.Value = value;
            _owner.Step = Step <= 0 ? 1 : Step;
            _owner.Slider.FixDomain();
            _owner.Slider.FixValue();
        }

        private Grasshopper.GUI.GH_DigitScroller CreateScroller(int y)
        {
            return new Grasshopper.GUI.GH_DigitScroller
            {
                Location = new Point(120, y),
                Size = new Size(222, 24),
                AllowTextInput = true,
                AllowRadixDrag = true,
                DecimalPlaces = 2,
                Digits = 8
            };
        }

        private static Label Label(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y + 4), AutoSize = true };
        }

        private static void SetScroller(Grasshopper.GUI.GH_DigitScroller scroller, decimal minimum, decimal maximum, decimal value)
        {
            scroller.DigitScroller.SetupScroller(minimum, maximum, Math.Max(minimum, Math.Min(maximum, value)));
        }

        private void SelectSchedule(AnimationTimerSchedule schedule)
        {
            var index = _scheduleBox.Items.IndexOf(schedule);
            _scheduleBox.SelectedIndex = index >= 0 ? index : 0;
        }

        private static System.Windows.Forms.Control Separator(int x, int y, int width)
        {
            return new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(x, y),
                Size = new Size(width, 2)
            };
        }
        private void UpdateDigitSettings()
        {
            var digits = Math.Max(0, Math.Min(6, Digits));
            var totalDigits = Math.Max(6, digits + 4);

            foreach (var scroller in new[] { numLower, numUpper, numRange, numStep, numValue })
            {
                scroller.Digits = totalDigits;
                scroller.DecimalPlaces = digits;
                scroller.Radix = totalDigits - digits;
                scroller.Invalidate();
            }
        }

        private void ClampDomain()
        {
            if (_updating) return;

            _updating = true;
            try
            {
                var lower = Math.Min(numLower.Value, numUpper.Value);
                var upper = Math.Max(numLower.Value, numUpper.Value);

                numValue.MinimumValue = lower;
                numValue.MaximumValue = upper;
                if (numValue.Value < lower) numValue.Value = lower;
                if (numValue.Value > upper) numValue.Value = upper;

                numRange.Value = Math.Max(0m, upper - lower);
            }
            finally
            {
                _updating = false;
            }
        }

        private void UpdateUpperFromRange()
        {
            if (_updating) return;

            _updating = true;
            try
            {
                if (numRange.Value < 0m) numRange.Value = 0m;
                numUpper.Value = numLower.Value + numRange.Value;

                var lower = Math.Min(numLower.Value, numUpper.Value);
                var upper = Math.Max(numLower.Value, numUpper.Value);
                numValue.MinimumValue = lower;
                numValue.MaximumValue = upper;
                if (numValue.Value < lower) numValue.Value = lower;
                if (numValue.Value > upper) numValue.Value = upper;
            }
            finally
            {
                _updating = false;
            }
        }
    }
}

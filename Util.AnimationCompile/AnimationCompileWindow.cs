using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    public class AnimationCompileWindow : Dialog<bool>
    {
        private readonly TextBox _inputFolderBox = new TextBox();
        private readonly TextBox _outputFolderBox = new TextBox();
        private readonly TextBox _outputNameBox = new TextBox();
        private readonly TextBox _framePrefixBox = new TextBox();
        private readonly NumericStepper _frameDigitsStepper = new NumericStepper();
        private readonly TextBox _frameExtensionBox = new TextBox();
        private readonly TextBox _exampleBox = new TextBox { ReadOnly = true };
        private readonly NumericStepper _frameDurationStepper = new NumericStepper();
        private readonly CheckBox _overwriteCheckBox = new CheckBox();
        private readonly NumericStepper _fromFrameStepper = new NumericStepper();
        private readonly NumericStepper _previewFrameStepper = new NumericStepper();
        private readonly NumericStepper _toFrameStepper = new NumericStepper();
        private readonly Button _startStopButton = new Button { Text = "Start" };
        private readonly PreviewCanvas _preview = new PreviewCanvas();
        private readonly UITimer _previewTimer = new UITimer();
        private bool _isPlaying;
        private Image _previewImage;

        public AnimationCompileWindow(AnimationSetting setting = null)
        {
            Title = "Animation Compile";
            ClientSize = new Size(720, 640);
            Resizable = true;
            Padding = new Padding(10);

            var currentSetting = setting ?? AnimationSetting.Default;
            SetInitialValues(currentSetting);

            _previewTimer.Interval = 1.0 / 12.0;
            _previewTimer.Elapsed += OnPreviewTimerElapsed;

            Content = CreateLayout();

            HookEvents();
            UpdateExample();
            UpdatePreview();
        }

        public AnimationSetting Setting { get; private set; }

        private void SetInitialValues(AnimationSetting setting)
        {
            _inputFolderBox.Text = setting.InputFolder ?? string.Empty;
            _outputFolderBox.Text = setting.OutputFolder ?? string.Empty;
            _outputNameBox.Text = string.IsNullOrWhiteSpace(setting.OutputName) ? "result.mov" : setting.OutputName;
            _framePrefixBox.Text = string.IsNullOrWhiteSpace(setting.FramePrefix) ? "Frame_" : setting.FramePrefix;

            ConfigureIntegerStepper(_frameDigitsStepper, Math.Max(1, setting.FrameDigits), 1, 12);
            _frameExtensionBox.Text = string.IsNullOrWhiteSpace(setting.FrameExtension)
                ? ".jpg"
                : "." + setting.FrameExtension.TrimStart('.');

            _frameDurationStepper.MinValue = 0.001;
            _frameDurationStepper.MaxValue = 10.0;
            _frameDurationStepper.DecimalPlaces = 4;
            _frameDurationStepper.Increment = 0.01;
            _frameDurationStepper.Value = setting.FrameDuration > 0 ? setting.FrameDuration : 0.03;

            _overwriteCheckBox.Text = "Overwrite";
            _overwriteCheckBox.Checked = setting.Overwrite;

            ConfigureIntegerStepper(_fromFrameStepper, 0, 0, 999999);
            ConfigureIntegerStepper(_previewFrameStepper, 23, 0, 999999);
            ConfigureIntegerStepper(_toFrameStepper, 200, 0, 999999);
        }

        private static void ConfigureIntegerStepper(NumericStepper stepper, int value, int min, int max)
        {
            stepper.MinValue = min;
            stepper.MaxValue = max;
            stepper.DecimalPlaces = 0;
            stepper.Increment = 1;
            stepper.Value = value;
        }

        private Eto.Forms.Control CreateLayout()
        {
            var inputBrowse = new Button { Text = "Browse..." };
            inputBrowse.Click += (sender, args) => BrowseFolder(_inputFolderBox, "Select input frame folder");

            var outputBrowse = new Button { Text = "Browse..." };
            outputBrowse.Click += (sender, args) => BrowseFolder(_outputFolderBox, "Select output folder");

            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, args) =>
            {
                Setting = CreateSettingFromControls();
                StopPreview();
                Close(true);
            };

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (sender, args) =>
            {
                StopPreview();
                Close(false);
            };

            _startStopButton.Click += (sender, args) => TogglePreviewPlayback();

            _preview.Size = new Size(680, 320);

            var layout = new DynamicLayout { Spacing = new Size(6, 6), Padding = new Padding(0) };

            layout.Add(CreateDirectoryRow("Input Directory", _inputFolderBox, inputBrowse));
            layout.Add(CreateDirectoryRow("Output Directory", _outputFolderBox, outputBrowse));
            layout.AddSeparateRow(new Label { Text = "Output Name" }, _outputNameBox);

            layout.AddSeparateRow(
                new Label { Text = "Frame Prefix" },
                _framePrefixBox,
                new Label { Text = "Frame Digits" },
                _frameDigitsStepper,
                new Label { Text = "Frame Extension" },
                _frameExtensionBox);

            layout.AddSeparateRow(new Label { Text = "Example" }, _exampleBox);
            layout.AddSeparateRow(new Label { Text = "Frame Duration" }, _frameDurationStepper, _overwriteCheckBox, null);
            layout.AddSeparateRow(
                new Label { Text = "From" },
                _fromFrameStepper,
                new Label { Text = "Frame" },
                _previewFrameStepper,
                new Label { Text = "To" },
                _toFrameStepper,
                _startStopButton);

            layout.AddSeparateRow(null, new Label { Text = "Preview" });
            layout.Add(_preview, yscale: true);
            layout.AddSeparateRow(null, okButton, cancelButton);

            return layout;
        }

        private static Eto.Forms.Control CreateDirectoryRow(string label, TextBox textBox, Button browseButton)
        {
            textBox.Width = 520;
            browseButton.Width = 96;

            return new TableLayout
            {
                Spacing = new Size(6, 0),
                Rows =
                {
                    new TableRow(
                        new Label { Text = label, VerticalAlignment = VerticalAlignment.Center },
                        new TableCell(textBox, true),
                        browseButton)
                }
            };
        }

        private void HookEvents()
        {
            _framePrefixBox.TextChanged += (sender, args) => UpdateExampleAndPreview();
            _frameExtensionBox.TextChanged += (sender, args) => UpdateExampleAndPreview();
            _frameDigitsStepper.ValueChanged += (sender, args) => UpdateExampleAndPreview();
            _inputFolderBox.TextChanged += (sender, args) => UpdatePreview();
            _previewFrameStepper.ValueChanged += (sender, args) =>
            {
                ClampPreviewFrame();
                UpdateExample();
                UpdatePreview();
            };
            _fromFrameStepper.ValueChanged += (sender, args) => ClampPreviewFrame();
            _toFrameStepper.ValueChanged += (sender, args) => ClampPreviewFrame();
        }

        private void BrowseFolder(TextBox target, string title)
        {
            using (var dialog = new SelectFolderDialog { Title = title })
            {
                if (!string.IsNullOrWhiteSpace(target.Text) && Directory.Exists(target.Text))
                    dialog.Directory = target.Text;

                var result = dialog.ShowDialog(this);
                if (result == DialogResult.Ok)
                    target.Text = dialog.Directory;
            }
        }

        private AnimationSetting CreateSettingFromControls()
        {
            var outputName = _outputNameBox.Text;
            if (string.IsNullOrWhiteSpace(outputName))
                outputName = "result.mov";
            if (!outputName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
                outputName += ".mov";

            return new AnimationSetting
            {
                InputFolder = _inputFolderBox.Text ?? string.Empty,
                OutputFolder = _outputFolderBox.Text ?? string.Empty,
                OutputName = outputName,
                FramePrefix = _framePrefixBox.Text ?? "Frame_",
                FrameDigits = Math.Max(1, (int)Math.Round(_frameDigitsStepper.Value)),
                FrameExtension = NormalizeExtension(_frameExtensionBox.Text),
                FrameDuration = Math.Max(0.001, _frameDurationStepper.Value),
                Overwrite = _overwriteCheckBox.Checked ?? false
            };
        }

        private void UpdateExample()
        {
            _exampleBox.Text = GetFrameFileName((int)Math.Round(_previewFrameStepper.Value));
        }

        private void UpdateExampleAndPreview()
        {
            UpdateExample();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var path = GetFramePath((int)Math.Round(_previewFrameStepper.Value));
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                SetPreviewImage(null);
                return;
            }

            try
            {
                SetPreviewImage(new Bitmap(path));
            }
            catch
            {
                SetPreviewImage(null);
            }
        }

        private void SetPreviewImage(Image image)
        {
            var oldImage = _previewImage;
            _previewImage = image;
            _preview.SetImage(_previewImage);
            oldImage?.Dispose();
        }

        private void TogglePreviewPlayback()
        {
            if (_isPlaying)
                StopPreview();
            else
                StartPreview();
        }

        private void StartPreview()
        {
            _isPlaying = true;
            _startStopButton.Text = "Stop";
            _previewTimer.Start();
        }

        private void StopPreview()
        {
            _isPlaying = false;
            _startStopButton.Text = "Start";
            _previewTimer.Stop();
        }

        private void OnPreviewTimerElapsed(object sender, EventArgs e)
        {
            var current = (int)Math.Round(_previewFrameStepper.Value);
            var from = (int)Math.Round(_fromFrameStepper.Value);
            var to = (int)Math.Round(_toFrameStepper.Value);
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            current++;
            if (current > to)
                current = from;

            _previewFrameStepper.Value = current;
            UpdatePreview();
        }

        private void ClampPreviewFrame()
        {
            var from = (int)Math.Round(_fromFrameStepper.Value);
            var to = (int)Math.Round(_toFrameStepper.Value);
            var min = Math.Min(from, to);
            var max = Math.Max(from, to);
            var current = (int)Math.Round(_previewFrameStepper.Value);

            if (current < min)
                _previewFrameStepper.Value = min;
            else if (current > max)
                _previewFrameStepper.Value = max;
        }

        private string GetFramePath(int frame)
        {
            var inputFolder = _inputFolderBox.Text;
            if (string.IsNullOrWhiteSpace(inputFolder))
                return string.Empty;

            return Path.Combine(inputFolder, GetFrameFileName(frame));
        }

        private string GetFrameFileName(int frame)
        {
            var prefix = _framePrefixBox.Text ?? string.Empty;
            var digits = Math.Max(1, (int)Math.Round(_frameDigitsStepper.Value));
            var extension = NormalizeExtension(_frameExtensionBox.Text);
            return $"{prefix}{frame.ToString().PadLeft(digits, '0')}.{extension}";
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "jpg";

            return extension.Trim().TrimStart('.');
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopPreview();
                _previewImage?.Dispose();
                _previewImage = null;
            }

            base.Dispose(disposing);
        }

        private class PreviewCanvas : Drawable
        {
            private const string MissingImageMessage = "Cannot catch the image";
            private Image _image;

            public void SetImage(Image image)
            {
                _image = image;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var bounds = new RectangleF(0, 0, Size.Width, Size.Height);
                e.Graphics.FillRectangle(Colors.LightGrey, bounds);

                if (_image != null)
                {
                    e.Graphics.DrawImage(_image, FitImageBounds(_image, bounds));
                    return;
                }

                var font = SystemFonts.Default(14);
                var textSize = e.Graphics.MeasureString(font, MissingImageMessage);
                var textPoint = new PointF(
                    bounds.Left + (bounds.Width - textSize.Width) / 2f,
                    bounds.Top + (bounds.Height - textSize.Height) / 2f);

                e.Graphics.DrawText(font, Colors.DimGray, textPoint, MissingImageMessage);
            }

            private static RectangleF FitImageBounds(Image image, RectangleF bounds)
            {
                var imageWidth = image.Width;
                var imageHeight = image.Height;
                if (imageWidth <= 0 || imageHeight <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
                    return bounds;

                var scale = Math.Min(bounds.Width / imageWidth, bounds.Height / imageHeight);
                var width = imageWidth * scale;
                var height = imageHeight * scale;

                return new RectangleF(
                    bounds.Left + (bounds.Width - width) / 2f,
                    bounds.Top + (bounds.Height - height) / 2f,
                    width,
                    height);
            }
        }
    }
}

using System;
using System.Drawing.Imaging;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Display;
using Woodpecker.Animation.Control.Camera;
using EtoContol = Eto.Forms.Control;
using SystemImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    public class RenderSettingWindow : Dialog<bool>
    {
        private readonly TextBox _outputPathBox = new TextBox();
        private readonly NumericStepper _sizeWStepper = new NumericStepper();
        private readonly NumericStepper _sizeHStepper = new NumericStepper();
        private readonly CheckBox _mirrorHCheckBox = new CheckBox { Text = "MirrowH" };
        private readonly CheckBox _mirrorVCheckBox = new CheckBox { Text = "MirrowV" };
        private readonly TextBox _framePrefixBox = new TextBox();
        private readonly NumericStepper _frameDigitsStepper = new NumericStepper();
        private readonly TextBox _frameExtensionBox = new TextBox();
        private readonly TextBox _frameNameBox = new TextBox { ReadOnly = true };
        private readonly CheckBox _overwriteCheckBox = new CheckBox { Text = "Overwrite" };
        private readonly CheckBox _transparentBackgroundCheckBox = new CheckBox { Text = "TransparentBackground" };
        private readonly CheckBox _drawGridCheckBox = new CheckBox { Text = "DrawGrid" };
        private readonly CheckBox _drawAxesCheckBox = new CheckBox { Text = "DrawAxes" };
        private readonly PreviewCanvas _preview = new PreviewCanvas();
        private readonly UITimer _previewTimer = new UITimer();
        private readonly CameraParameter cameraParameter = null;
        private Image _previewImage;
        public RenderSettingWindow(CameraParameter cameraParameter, RenderSetting setting = null) : this(setting)
        {
            this.cameraParameter = cameraParameter ?? null;
        }
        public RenderSettingWindow(RenderSetting setting = null)
        {
            Title = "Render Setting";
            ClientSize = new Size(720, 560);
            Resizable = true;
            Padding = new Padding(10);

            SetInitialValues(setting ?? RenderSetting.Default);

            _previewTimer.Interval = 0.25;
            _previewTimer.Elapsed += (sender, args) =>
            {
                _previewTimer.Stop();
                UpdatePreview();
            };

            Content = CreateLayout();

            HookEvents();
            UpdateFrameName();
            SchedulePreviewUpdate();
        }

        public RenderSetting Setting { get; private set; }

        private void SetInitialValues(RenderSetting setting)
        {
            _outputPathBox.Text = setting.OutputPath ?? string.Empty;

            ConfigureIntegerStepper(_sizeWStepper, Math.Max(24, setting.SizeW), 24, 20000);
            ConfigureIntegerStepper(_sizeHStepper, Math.Max(24, setting.SizeH), 24, 20000);

            _mirrorHCheckBox.Checked = setting.MirrowH;
            _mirrorVCheckBox.Checked = setting.MirrowV;

            _framePrefixBox.Text = string.IsNullOrWhiteSpace(setting.FramePrefix) ? "Frame_" : setting.FramePrefix;
            ConfigureIntegerStepper(_frameDigitsStepper, Math.Max(1, setting.FrameDigits), 1, 12);
            _frameExtensionBox.Text = "." + NormalizeExtension(setting.FrameExtension);

            _overwriteCheckBox.Checked = setting.Overwrite;
            _transparentBackgroundCheckBox.Checked = setting.TransparentBackground;
            _drawGridCheckBox.Checked = setting.DrawGrid;
            _drawAxesCheckBox.Checked = setting.DrawAxes;
        }

        private static void ConfigureIntegerStepper(NumericStepper stepper, int value, int min, int max)
        {
            stepper.MinValue = min;
            stepper.MaxValue = max;
            stepper.DecimalPlaces = 0;
            stepper.Increment = 1;
            stepper.Value = value;
        }

        private EtoContol CreateLayout()
        {
            var outputBrowse = new Button { Text = "Browse..." };
            outputBrowse.Click += (sender, args) => BrowseFolder(_outputPathBox, "Select output folder");

            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, args) =>
            {
                Setting = CreateSettingFromControls();
                Close(true);
            };

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (sender, args) => Close(false);

            _preview.Size = new Size(680, 280);

            var layout = new DynamicLayout { Spacing = new Size(6, 6), Padding = new Padding(0) };

            layout.Add(CreateDirectoryRow("OutputPath", _outputPathBox, outputBrowse));
            layout.AddSeparateRow(new Label { Text = "SizeW" }, _sizeWStepper, new Label { Text = "SizeH" }, _sizeHStepper, null);
            layout.AddSeparateRow(_mirrorHCheckBox, _mirrorVCheckBox, null);
            layout.AddSeparateRow(
                new Label { Text = "FramePrefix" },
                _framePrefixBox,
                new Label { Text = "FrameDigits" },
                _frameDigitsStepper,
                new Label { Text = "FrameExtension" },
                _frameExtensionBox);
            layout.AddSeparateRow(new Label { Text = "FrameName" }, _frameNameBox);
            layout.AddSeparateRow(_overwriteCheckBox, null);
            layout.AddSeparateRow(_transparentBackgroundCheckBox, _drawGridCheckBox, _drawAxesCheckBox, null);
            layout.Add(new Panel { Height = 1, BackgroundColor = Colors.Gray });
            layout.AddSeparateRow(null, new Label { Text = "Preview" });
            layout.Add(_preview, yscale: true);
            layout.AddSeparateRow(null, okButton, cancelButton);

            return layout;
        }

        private static EtoContol CreateDirectoryRow(string label, TextBox textBox, Button browseButton)
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
            _sizeWStepper.ValueChanged += (sender, args) => SchedulePreviewUpdate();
            _sizeHStepper.ValueChanged += (sender, args) => SchedulePreviewUpdate();
            _mirrorHCheckBox.CheckedChanged += (sender, args) => SchedulePreviewUpdate();
            _mirrorVCheckBox.CheckedChanged += (sender, args) => SchedulePreviewUpdate();
            _transparentBackgroundCheckBox.CheckedChanged += (sender, args) => UpdateFrameNameAndPreview();
            _drawGridCheckBox.CheckedChanged += (sender, args) => SchedulePreviewUpdate();
            _drawAxesCheckBox.CheckedChanged += (sender, args) => SchedulePreviewUpdate();
            _framePrefixBox.TextChanged += (sender, args) => UpdateFrameNameAndPreview();
            _frameDigitsStepper.ValueChanged += (sender, args) => UpdateFrameNameAndPreview();
            _frameExtensionBox.TextChanged += (sender, args) => UpdateFrameNameAndPreview();
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

        private RenderSetting CreateSettingFromControls()
        {
            return new RenderSetting
            {
                OutputPath = _outputPathBox.Text ?? string.Empty,
                SizeW = Math.Max(24, (int)Math.Round(_sizeWStepper.Value)),
                SizeH = Math.Max(24, (int)Math.Round(_sizeHStepper.Value)),
                MirrowH = _mirrorHCheckBox.Checked ?? false,
                MirrowV = _mirrorVCheckBox.Checked ?? false,
                FramePrefix = _framePrefixBox.Text ?? "Frame_",
                FrameDigits = Math.Max(1, (int)Math.Round(_frameDigitsStepper.Value)),
                FrameExtension = NormalizeExtension(_frameExtensionBox.Text),
                Overwrite = _overwriteCheckBox.Checked ?? false,
                TransparentBackground = _transparentBackgroundCheckBox.Checked ?? false,
                DrawGrid = _drawGridCheckBox.Checked ?? false,
                DrawAxes = _drawAxesCheckBox.Checked ?? false
            };
        }

        private void UpdateFrameNameAndPreview()
        {
            UpdateFrameName();
            SchedulePreviewUpdate();
        }

        private void UpdateFrameName()
        {
            var previewSetting = CreateSettingFromControls();
            _frameNameBox.Text = GetFrameName(previewSetting, 1);
        }

        private void SchedulePreviewUpdate()
        {
            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private void UpdatePreview()
        {
            var rh_viewIndex = -1;
            var rh_view = RhinoDoc.ActiveDoc?.Views.ActiveView;
            if (rh_view == null)
            {
                SetPreviewImage(null);
                return;
            }

            if(cameraParameter != null)
            {
                rh_viewIndex = RhinoDoc.ActiveDoc.NamedViews.Add("__temp_view__", rh_view.ActiveViewportID);
                if(rh_viewIndex < 0)
                {
                    RhinoApp.WriteLine("Frame failed to render: temporary view cannot be created.");
                }
                rh_view.ActiveViewport.SetViewProjection(cameraParameter.viewportInfo, false);
                rh_view.Redraw();
            }

            try
            {
                var previewSetting = CreateSettingFromControls();
                FitPreviewSize(previewSetting);
                var frameName = GetFrameName(previewSetting, 1);

                using (var bitmap = CreatePreviewBitmap(rh_view, previewSetting, frameName))
                {
                    if (bitmap == null)
                    {
                        SetPreviewImage(null);
                        return;
                    }

                    ApplyMirror(bitmap, previewSetting);
                    SetPreviewImage(ToEtoImage(bitmap));
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Render setting preview failed: {ex.Message}");
                SetPreviewImage(null);
            }
            finally
            {
                if(rh_viewIndex >= 0)
                {
                    RhinoDoc.ActiveDoc.NamedViews.Restore(rh_viewIndex, rh_view.ActiveViewport);
                    RhinoDoc.ActiveDoc.NamedViews.Delete(rh_viewIndex);
                    rh_view.Redraw();
                }
            }
        }

        private static System.Drawing.Bitmap CreatePreviewBitmap(RhinoView view, RenderSetting setting, string frameName)
        {
            try
            {
                return RenderUtil.CreateFrame(view, setting, frameName);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Render setting preview ViewCapture failed, fallback to DisplayPipeline. {ex.Message}");
                return RenderUtil.CreateFrame(view?.ActiveViewport, setting, frameName);
            }
        }

        private void SetPreviewImage(Image image)
        {
            var oldImage = _previewImage;
            _previewImage = image;
            _preview.SetImage(_previewImage);
            oldImage?.Dispose();
        }

        private void FitPreviewSize(RenderSetting setting)
        {
            var sourceWidth = Math.Max(24, setting.SizeW);
            var sourceHeight = Math.Max(24, setting.SizeH);
            var maxWidth = Math.Max(160, _preview.Width > 0 ? _preview.Width : 680);
            var maxHeight = Math.Max(120, _preview.Height > 0 ? _preview.Height : 280);
            var scale = Math.Min(maxWidth / (double)sourceWidth, maxHeight / (double)sourceHeight);
            scale = Math.Min(1.0, scale);

            setting.SizeW = Math.Max(24, (int)Math.Round(sourceWidth * scale));
            setting.SizeH = Math.Max(24, (int)Math.Round(sourceHeight * scale));
        }

        private static void ApplyMirror(System.Drawing.Bitmap bitmap, RenderSetting setting)
        {
            if (setting.MirrowH && setting.MirrowV)
                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipXY);
            else if (setting.MirrowH)
                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
            else if (setting.MirrowV)
                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
        }

        private static Image ToEtoImage(System.Drawing.Bitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, SystemImageFormat.Png);
            stream.Position = 0;
            using (stream)
            {
                return new Bitmap(stream);
            }
        }

        private static string GetFrameName(RenderSetting setting, int frame)
        {
            var frameNumber = frame.ToString().PadLeft(setting.FrameDigits, '0');
            return $"{setting.FramePrefix}{frameNumber}.{setting.FrameExtension}";
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return "jpg";

            return extension.Trim().TrimStart('.').ToLowerInvariant();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewTimer.Stop();
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

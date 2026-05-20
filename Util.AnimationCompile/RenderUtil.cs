using System;
using Rhino;
using Woodpecker.Animation.Util.IO;
using System.IO;
using Rhino.Display;
using System.Drawing;
using Grasshopper.GUI;
using System.Drawing.Imaging;
using Grasshopper.Kernel;
using Rhino.Render;
using Woodpecker.Animation.Control.Camera;
using Rhino.DocObjects;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    public static class RenderUtil
    {
        private static void CheckRenderSetting(RenderSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.OutputPath))
            {
                var newOutputPath = ProjectAppManager.Get_DataRoot + "/AnimationFrames";
                setting.OutputPath = newOutputPath;
            }
        }
        private static void ValidateSetting(RenderSetting setting)
        {
            if (string.IsNullOrWhiteSpace(setting.OutputPath))
            {
                throw new DirectoryNotFoundException("Output folder does not exist.");
            }
            if (setting.FrameDigits <= 0)
                throw new ArgumentException("FrameDigits must be greater than 0.", nameof(setting.FrameDigits));
            setting.FramePrefix = setting.FramePrefix ?? string.Empty;

            setting.FrameExtension = string.IsNullOrWhiteSpace(setting.FrameExtension)
                ? "jpg"
                : setting.FrameExtension.TrimStart('.');

        }
        public static bool Render_A_Image_From_Camera(string ViewportName, RenderSetting setting, out Bitmap bitmap)
        {
            var cameraParam = new CameraParameter(ViewportName);
            return Render_A_Image_From_Camera(cameraParam, setting, out bitmap);
        }
        public static bool Render_A_Image_From_Camera(CameraParameter cameraParameter, RenderSetting setting, out Bitmap bitmap)
        {
            /// we need to shift current viewport to the cameraParameter and recover it after renders
            bitmap = null;
            var doc = RhinoDoc.ActiveDoc;
            if(doc == null)
            {
                throw new Exception("RhinoDoc.ActiveDoc cannot be null");
            }
            var activeView = doc.Views.ActiveView;
            var tempviewIndex = doc.NamedViews.Add("__temp_view__", activeView.ActiveViewportID);
            if (tempviewIndex < 0)
            {
                RhinoApp.WriteLine("Frame failed to render: temporary view cannot be created.");
                return false;
            }

            try
            {
                activeView.ActiveViewport.SetViewProjection(cameraParameter.viewportInfo, false);
                activeView.Redraw();
                return Render_A_Image(setting, out bitmap);
            }
            finally
            {
                doc.NamedViews.Restore(tempviewIndex, activeView.ActiveViewport);
                doc.NamedViews.Delete(tempviewIndex);
                activeView.Redraw();
            }
        }
        public static bool SaveBitmap(Bitmap bitmap, RenderSetting setting, string frameName)
        {
            if(bitmap == null)
                return false;
             var format = GetImageFormat(setting.FrameExtension);
            var path = Path.Combine(setting.OutputPath, frameName);
            bitmap.Save(path, format);
            RhinoApp.WriteLine($"View is saved in disk {path}");
            return true;
        }
        public static bool Render_A_Image(RenderSetting setting, out Bitmap bitmap)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            RhinoApp.WriteLine("Start Animation Render");
            CheckRenderSetting(setting);
            Directory.CreateDirectory(setting.OutputPath);
            ValidateSetting(setting);

            var frameName = GetFrameName(setting, 1);
            //var path = Path.Combine(setting.OutputPath, frameName);

            var doc = RhinoDoc.ActiveDoc;
            var activeView = doc?.Views.ActiveView;
            var viewport = activeView?.ActiveViewport;
            if (viewport == null)
            {
                RhinoApp.WriteLine("Frame failed to render: active viewport cannot be found.");
                bitmap = null;
                return false;
            }
            try
            {
                bitmap = CreateFrame(activeView, setting, frameName);
                if (bitmap == null)
                {
                    RhinoApp.WriteLine("Frame failed to render.");
                    return false;
                }

                ApplyMirror(bitmap, setting);
            }
            catch(Exception ex)
            {
                RhinoApp.WriteLine($"ViewCapture failed, fallback to DisplayPipeline. {ex.Message}");
                bitmap = CreateFrame(viewport, setting, frameName);
                if (bitmap == null)
                {
                    RhinoApp.WriteLine("Frame failed to render.");
                    return false;
                }

                ApplyMirror(bitmap, setting);
            }

            RhinoApp.WriteLine($"Frame is successfully render");
            return true;
        }
        public static string GetFrameName(RenderSetting setting, int frame)
        {
            var frameNumber = frame.ToString().PadLeft(setting.FrameDigits, '0');
            return $"{setting.FramePrefix}{frameNumber}.{setting.FrameExtension}";
        }
        public static Bitmap CreateFrame(RhinoView m_view, RenderSetting setting, string frameName)
        {
            if (m_view == null)
                return null;
            var capture = new ViewCapture
            {
                Width = setting.ViewSize.Width,
                Height = setting.ViewSize.Height,
                TransparentBackground = setting.TransparentBackground,
                DrawGrid = setting.DrawGrid,
                DrawAxes = setting.DrawAxes,
                ScaleScreenItems = false
            };
            
            var bitmap = capture.CaptureToBitmap(m_view);
            if (bitmap == null)
                return null;
            if (setting.AddFrameLabel)
                DrawFrameLabel(bitmap, frameName);
            return bitmap;
        }
        private static void ApplyMirror(Bitmap bitmap, RenderSetting setting)
        {
            if (bitmap == null)
                return;
            if (setting.MirrowH && setting.MirrowV)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            else if (setting.MirrowH)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else if (setting.MirrowV)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
        private static void DrawFrameLabel(Bitmap bitmap, string frameName)
        {
            checked
            {
                SizeF sizeF = GH_FontServer.MeasureString(frameName, GH_FontServer.Standard);
                var rect = new Rectangle(-1, Convert.ToInt32((float)bitmap.Height - sizeF.Height) - 5, bitmap.Width + 2, Convert.ToInt32(sizeF.Height) + 10);
                using (var graphic = Graphics.FromImage(bitmap))
                using (var solidBrush = new SolidBrush(Color.FromArgb(150, Color.White)))
                {
                    graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    graphic.TextRenderingHint = GH_TextRenderingConstants.GH_CrispText;
                    graphic.FillRectangle(solidBrush, rect);
                    graphic.DrawLine(Pens.Black, rect.Left, rect.Y, rect.Right, rect.Y);
                    graphic.DrawString(frameName, GH_FontServer.Standard, Brushes.Black, 2f, rect.Y + 2);
                }
            }
        }
        public static Bitmap CreateFrame(RhinoViewport m_viewport, RenderSetting setting, string frameName)
        {
            if (m_viewport == null)
            {
                return null;
            }

            var bitmap = DisplayPipeline.DrawToBitmap(m_viewport, setting.ViewSize.Width, setting.ViewSize.Height);
            if (bitmap == null)
            {
                return null;
            }
            if (setting.AddFrameLabel)
                DrawFrameLabel(bitmap, frameName);
            return bitmap;
        }
        private static ImageFormat GetImageFormat(string extension)
        {
            switch ((extension ?? string.Empty).TrimStart('.').ToLowerInvariant())
            {
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                case "tif":
                case "tiff":
                    return ImageFormat.Tiff;
                case "png":
                    return ImageFormat.Png;
                case "jpg":
                case "jpeg":
                default:
                    return ImageFormat.Jpeg;
            }
        }
    }
}

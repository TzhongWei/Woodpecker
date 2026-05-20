using System;
using System.Configuration;
using System.Drawing;
using Eto.Forms;
using Newtonsoft.Json;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    public class RenderSetting
    {
        public Size ViewSize
        {
            get
            {
                var W = Math.Max(SizeW, 24);
                var H = Math.Max(SizeH, 24);
                var _size = new Size(W, H);
                return _size;
            }
        }
        public int SizeW { get; set; } = 1920;
        public int SizeH { get; set; } = 1080;
        public string OutputPath { get; set; } = "";
        public string FramePrefix { get; set; } = "Frame_";
        public int FrameDigits { get; set; } = 5;
        private string _frameExtension = "jpg";
        public string FrameExtension
        {
            get
            {
                _frameExtension = string.IsNullOrWhiteSpace(_frameExtension)
                    ? "jpg"
                    : _frameExtension.TrimStart('.').ToLowerInvariant();

                if (TransparentBackground &&
                    _frameExtension != "png" &&
                    _frameExtension != "tif" &&
                    _frameExtension != "tiff")
                {
                    _frameExtension = "png";
                }

                return this._frameExtension;
            }
            set
            {
                this._frameExtension = string.IsNullOrWhiteSpace(value)
                    ? "jpg"
                    : value.TrimStart('.').ToLowerInvariant();
            }
        }
        public bool Overwrite { get; set; } = true;
        public bool MirrowV { get; set; } = false;
        public bool MirrowH { get; set; } = false;
        public bool TransparentBackground { get; set; } = false;
        public bool DrawGrid { get; set; } = false;
        public bool DrawAxes { get; set; } = false;
        public bool AddFrameLabel { get; set; } = false;
        public static RenderSetting Default
        {
            get
            {
                var setting = new RenderSetting();
                setting.OutputPath = ProjectAppManager.Get_DataRoot + "/AnimationFrames";
                setting.SizeW = 1920;
                setting.SizeH = 1080;
                setting.FrameDigits = 5;
                setting.FramePrefix = "Frame_";
                setting._frameExtension = "jpg";
                setting.Overwrite = true;
                setting.MirrowH = false;
                setting.MirrowV = false;
                setting.TransparentBackground = false;
                setting.DrawGrid = false;
                setting.DrawAxes = false;
                setting.AddFrameLabel = false;
                return setting;
            }
        }
        public RenderSetting()
        {
        }
        public RenderSetting Clone()
        {
            return new RenderSetting
            {
                OutputPath = this.OutputPath,
                SizeW = this.SizeW,
                SizeH = this.SizeH,
                FrameDigits = this.FrameDigits,
                FramePrefix = this.FramePrefix,
                _frameExtension = this.FrameExtension,
                Overwrite = this.Overwrite,
                MirrowH = this.MirrowH,
                MirrowV = this.MirrowV,
                TransparentBackground = this.TransparentBackground,
                DrawGrid = this.DrawGrid,
                DrawAxes = this.DrawAxes,
                AddFrameLabel = this.AddFrameLabel,
            };
        }
        public string ToJson() => JsonConvert.SerializeObject(this);
        public static bool FromJson(string json, out RenderSetting setting)
        {
            var value = JsonConvert.DeserializeObject<RenderSetting>(json);
            if (value == null)
            {
                setting = Default;
                return false;
            }
            else
            {
                setting = value;
                return true;
            }
        }
    }
}

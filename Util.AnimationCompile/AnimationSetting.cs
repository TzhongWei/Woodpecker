using Newtonsoft.Json;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.Util.AnimationCompile
{
    public class AnimationSetting
    {
        public static AnimationSetting Default => new AnimationSetting()
        {
            InputFolder = ProjectAppManager.Get_DataRoot + "/AnimationFrames",
            OutputFolder = ProjectAppManager.Get_DataRoot + "/AnimationOutput",
            OutputName = "result.mov",
            FramePrefix = "Frame_",
            FrameDigits = 5,
            FrameExtension = "jpg",
            FrameDuration = 1 / 30.0,
            Overwrite = true
        };
        public string ToJson() => JsonConvert.SerializeObject(this);
        public static bool FromJson(string json, out AnimationSetting setting) 
        {
            var value = JsonConvert.DeserializeObject<AnimationSetting>(json);
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
        public string InputFolder { get; set; } = "";

        public string OutputFolder { get; set; } = "";

        public string OutputName { get; set; } = "result.mov";

        public string FramePrefix { get; set; } = "Frame_";

        public int FrameDigits { get; set; } = 5;

        public string FrameExtension { get; set; } = "jpg";

        public double FrameDuration { get; set; } = 1.0 / 30.0;

        public bool Overwrite { get; set; } = true;

        public double FPS => 1.0 / FrameDuration;
    }
}

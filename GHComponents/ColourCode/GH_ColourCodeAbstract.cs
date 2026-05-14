using System.Collections.Generic;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public abstract class GH_ColourCodeAbstract: GH_Component, IUpdateDependent
    {
        public string UpdateTag => "ColourCodeDependent";
        public Dictionary<string, object> SettingPair = new Dictionary<string, object>();
        public GH_ColourCodeAbstract(string Name, string NickName, string Description):base(Name, NickName, Description, "Woodpecker", "ColourCode")
        {
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("Path", ProjectAppManager.ColourCodePath);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            var path = "";
            if(reader.TryGetString("Path", ref path))
            {
                ColourCodeIO.ReadColourFromPath(path);
            }
            return base.Read(reader);
        }
    }
}
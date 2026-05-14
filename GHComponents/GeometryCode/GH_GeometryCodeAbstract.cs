using GH_IO.Serialization;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public abstract class GH_GeometryCodeAbstract : GH_Component, IUpdateDependent
    {
        public string UpdateTag => "GeometryCode";
        public GH_GeometryCodeAbstract(string Name, string NickName, string Description):base(Name, NickName, Description, "Woodpecker", "GeometryCode")
        {
            
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("Path", ProjectAppManager.GeometryCodePath);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            var path = "";
            if(reader.TryGetString("Path", ref path))
            {
                GeometryCodeIO.ReadGeometryFromPath(path);
            }
            return base.Read(reader);
        }
    }
}
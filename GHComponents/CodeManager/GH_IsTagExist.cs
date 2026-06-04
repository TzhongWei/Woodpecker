using Woodpecker.Animation.CodeManager;
using System;
using Grasshopper.Kernel;
using System.Linq;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_IsTagExist: GH_TagChannel_Abstract
    {
        public GH_IsTagExist():base("Is Tag Existed", "Tag Existed", "Check whether the tag is created and used"){}
        public override Guid ComponentGuid => new Guid("d1c8b9e7-5a3c-4f0b-9c8e-2a1b6f3e4d5f");
        public override RemoteType ChannelType => RemoteType.Process;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "The label for a channel, which will be used to match with the corresponding output or input channel", GH_ParamAccess.item);
        }
        public override bool IsPrimaryInstance()
        {
            return true;
        }  
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("IsExist", "Exist", "Whether the tag is created and used", GH_ParamAccess.item);
            pManager.AddTextParameter("ChannelType", "CType", "The channel type of the tag is used for input only or not", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string tag = "";
            DA.GetData("Tag", ref tag);
            this.SingletonTag = tag;
            var checkCount = checkTagCount();
            DA.SetData("IsExist", checkCount > 0);
            DA.SetData("ChannelType", checkCount == 1? "A tag is created but not used" : "The tag is created and used for input or output");
        }
        protected int checkTagCount()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return -1;
            var sameTag = doc.Objects
            .OfType<GH_TagChannel_Abstract>()
            .Where(x => x.SingletonTag == this.SingletonTag)
            .ToList();
            return sameTag.Count;
        }
    }
}
using Woodpecker.Animation.CodeManager;
using System;
using Grasshopper.Kernel;
using System.Linq;
using System.Drawing;

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
            
            if(!checkCount)
            {
                DA.SetData("IsExist", false);
                DA.SetData("ChannelType", CheckTagCount() > 0 ? "The tag is used in Tag output Channel, which is invalid": "Error: Tag is not created");
                return;
            }
            else
            {
                DA.SetData("ChannelType", CheckTagCount() == 1? "A tag is created but not used" : "The tag is created and used for input or output");
                DA.SetData("IsExist", true);
            }
        }
        protected int CheckTagCount()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return -1;
            var sameTag = doc.Objects
            .OfType<GH_TagChannel_Abstract>()
            .Where(x => x.SingletonTag == this.SingletonTag && x.InstanceGuid != this.InstanceGuid)
            .ToList();

            return sameTag.Count;
         }
        protected bool checkTagCount()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;
            var sameTag = doc.Objects
            .OfType<GH_TagChannel_Abstract>()
            .Where(x => x.SingletonTag == this.SingletonTag && x.ChannelType == RemoteType.Input && x.InstanceGuid != this.InstanceGuid)
            .ToList();


            return sameTag.Count == 1;
        }
        protected override Bitmap Icon => Properties.Resources.GH_Is_Tag;
    }
}
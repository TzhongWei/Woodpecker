using System;
using System.Linq;
using Grasshopper.Kernel;
using Microsoft.VisualBasic;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_TimeSlotChannel_OUT : GH_TimeSlotChannel_Abstract
    {
        public GH_TimeSlotChannel_OUT():base("Global T output", "TSlot_Out", "", "Util"){}
        protected GH_TimeSlotChannel_OUT(string Name, string NickName, string Description, string Subcategory) : base(Name, NickName, Description, Subcategory){}
        public override Guid ComponentGuid => new Guid("f83a6ffa-b24e-4ac0-a001-bb78709b47ef");
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string tag = "";
            DA.GetData("Tag", ref tag);
            DA.DisableGapLogic();
            if(string.IsNullOrWhiteSpace(tag)) return;

            this.SingletonTag = tag;

            if(!IsPrimaryInstance() || !GetValue(out var t_Value))
            {
                this.AddRuntimeMessage( GH_RuntimeMessageLevel.Error, $"Global T value must be a number or the tag {this.SingletonTag} cannot be found");
                DA.SetData("Global T", -1);
                return;
            }
            

            DA.SetData("Global T", t_Value);

        }
        protected bool GetValue(out double value)
        {
            var doc = this.OnPingDocument();
            var target_Component = doc.Objects.OfType<GH_TagChannel_Abstract>().FirstOrDefault(x => x.ChannelType == RemoteType.Input && x.SingletonTag == this.SingletonTag);
            if(target_Component == null || target_Component?.tagChannel == null  || !target_Component.tagChannel.HasValidChannel())
            {
                value = -1;
                return false;
            }
            if(target_Component is GH_TimeSlotChannel_Abstract gH_TimeSlotChannel_Abstract)
            {
                this.tagChannel = gH_TimeSlotChannel_Abstract.tagChannel;
                value = gH_TimeSlotChannel_Abstract.GetTimeSlotTagChannel().Value;
            }
            else
            {
                this.tagChannel = target_Component.tagChannel;
                var timeSlot = (TimeSlotTagChannel) this.tagChannel;
                if( timeSlot.Value == -1)
                {
                    value = -1;
                    return false;
                }
                value = timeSlot.Value;
            }
            return true;
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "", GH_ParamAccess.item, "Global_T");
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Global T", "T", "", GH_ParamAccess.item);
        }
        public override bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if(doc == null) return false;

            return doc.Objects.OfType<GH_TagChannel_Abstract>()
            .Any(x => x.ChannelType == RemoteType.Input 
            && x.SingletonTag == this.SingletonTag);
        }
        public override RemoteType ChannelType => RemoteType.Output;
    }
}
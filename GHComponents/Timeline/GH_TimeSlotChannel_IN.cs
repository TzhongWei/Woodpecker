using System;
using System.Linq;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_TimeSlotChannel_IN : GH_TimeSlotChannel_Abstract
    {
        public GH_TimeSlotChannel_IN() : base("Global T input", "TSlot_In", "Publish a named global timeline value to a time slot channel.", "Util") { }
        public override RemoteType ChannelType => RemoteType.Input;
        public override Guid ComponentGuid => new Guid("9cabd915-8c49-4e0b-b2bf-88c0766c9534");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "Time slot tag used to identify the published value.", GH_ParamAccess.item, "Global_T");
            pManager.AddNumberParameter("Global T", "T", "Timeline value to publish to the time slot channel.", GH_ParamAccess.item, 0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

        }
        private bool UpdateRemoteOutput()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;

            doc.ScheduleSolution(1, d =>
            {
                foreach (var outComp in doc.Objects.OfType<GH_TagChannel_Abstract>().Where(x
                => (x.ChannelType == RemoteType.Output || x.ChannelType == RemoteType.Process) && x.SingletonTag == this.SingletonTag))
                {
                    outComp.ExpireSolution(true);
                }
            });
            return true;
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string tag = "";
            double value = 0.0;
            DA.GetData("Tag", ref tag);
            DA.GetData("Global T", ref value);
            var oldTag = this.SingletonTag;
            var timeslotchannel = new TimeSlotTagChannel(tag);
            timeslotchannel.SetValue(value);
            this.tagChannel = timeslotchannel;
            this.SingletonTag = tag;
            UpdateRemoteOutput();
            if(oldTag != tag)
            {
                UpdateProcessOutput(oldTag);
            }
        }
        public override bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;
            var sameTag = doc.Objects
            .OfType<GH_TagChannel_Abstract>()
            .Where(x => x.ChannelType == this.ChannelType)
            .Where(x => x.SingletonTag == this.SingletonTag)
            .OrderBy(x => x.InstanceGuid).ToList();
            return sameTag.Count == 1;
        }
    }
}

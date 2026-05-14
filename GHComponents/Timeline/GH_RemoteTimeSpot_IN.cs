using System;
using System.Linq;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Control.Timeline;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    [Obsolete]
    public class GHRemoteTimeSpotIN : GH_RemoteTimeSpotAbstract
    {
        public override Guid ComponentGuid => new Guid("167ed580-0a6a-4e54-83f6-97bba43a9701");

        public override RemoteType _remoteType => RemoteType.Input;

        public override bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;
            var sameTag = doc.Objects
            .OfType<GH_RemoteTimeSpotAbstract>()
            .Where(x => x._remoteType == this._remoteType)
            .Where(x => x.SingletonTag == this.SingletonTag)
            .OrderBy(x => x.InstanceGuid).ToList();
            return sameTag.Count == 1;

        }
        public GHRemoteTimeSpotIN() : base("Remote Time spot input", "In_TSpot", "Connect a time spot without Grasshopper wires and label the time spot with a tag")
        {

        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "The label for a time spot", GH_ParamAccess.item);
            pManager.AddNumberParameter("Time spot", "TSpot", "A number input with the tag", GH_ParamAccess.item);
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
                foreach (var outComp in doc.Objects.OfType<GH_RemoteTimeSpotAbstract>().Where(
                    x => x._remoteType == RemoteType.Output && x.SingletonTag == this.SingletonTag)
                )
                {
                    outComp.ExpireSolution(false);
                }
            });
            return true;
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string tag = "";
            double timespot = 0.0;

            DA.GetData("Tag", ref tag);
            DA.GetData("Time spot", ref timespot);

            this.RemoteTimeData = new RemoteTime(tag);
            this.SingletonTag = tag;
            this.RemoteTimeData.Value = timespot;
            UpdateRemoteOutput();
            this.MessageSetup();
        }

    }
}
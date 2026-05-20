using System;
using System.Linq;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    [Obsolete]
    /// <summary>
    /// Get a time spot without grasshopper wires from a label with a tag. Inputs include Tag. Outputs include Value.
    /// </summary>
    public class GH_RemoteTimeSpot_OUT : GH_RemoteTimeSpotAbstract
    {
        public GH_RemoteTimeSpot_OUT() : base("Remote Time spot output", "Out_TSpot", "Get a time spot without grasshopper wires from a label with a tag") { }
        public override Guid ComponentGuid => new Guid("be1f0302-b9d9-4b13-9345-b5925880fdec");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "The labelled for a time spot", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "TSpot", "If find the tag, the value will larger than 0", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string tag = "";
            if(!DA.GetData("Tag", ref tag))
            {
                return;
            }
            if(string.IsNullOrWhiteSpace(tag))
            {
                return;
            }
            this.SingletonTag = tag;
            if (!IsPrimaryInstance())
            {
                DA.SetData("Value", -1);
                return;
            }
            var doc = this.OnPingDocument();
            var target_Component = doc.Objects
            .OfType<GH_RemoteTimeSpotAbstract>()
            .FirstOrDefault(x =>
            x._remoteType == RemoteType.Input &&
            x.SingletonTag == this.SingletonTag);

            if(target_Component == null || target_Component.RemoteTimeData == null)
            {
                DA.SetData("Value", -1);
                return;
            }

            this.RemoteTimeData = target_Component.RemoteTimeData;
            this.MessageSetup();
            DA.SetData("Value", this.RemoteTimeData.Value);
        }

        public override bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;
            return doc.Objects
            .OfType<GH_RemoteTimeSpotAbstract>()
            .Any(x => x._remoteType == RemoteType.Input &&
            x.SingletonTag == this.SingletonTag);
        }

        public override RemoteType _remoteType => RemoteType.Output;


    }
}
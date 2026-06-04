using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Timeline;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.CodeManager;


namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Reads general data from a tag channel with the same tag. Output nicknames are matched to stored data keys from Tag Channel Input, so each output receives the branch associated with that name. Inputs include Tag. Outputs include General Data A.
    /// </summary>
    public class GH_TagChannel_Out : GH_TagChannel_Abstract, IGH_VariableParameterComponent
    {
        public GH_TagChannel_Out() : base("Tag Channel Output", "Out_Tag", "output values based on the input tag") { }
        public override RemoteType ChannelType => RemoteType.Output;

        public override Guid ComponentGuid => new Guid("424aaa53-1ff2-4ab6-8f09-bfb52c63a6ad");

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output;
        }
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return this.CanInsertParameter(side, index);
        }
        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Output;
        }
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return this.CanRemoveParameter(side, index);
        }
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject
            {
                Name = "General Data",
                NickName = string.Empty,
                Description = "output data with a tag",
                Access = GH_ParamAccess.tree,
                Optional = true
            };
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return this.CreateParameter(side, index);
        }
        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return this.DestroyParameter(side, index);
        }
        public override bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;
            return doc.Objects
            .OfType<GH_TagChannel_Abstract>()
            .Any(x => x.ChannelType == RemoteType.Input &&
            x.SingletonTag == this.SingletonTag);
        }

        public void VariableParameterMaintenance()
        {
            checked
            {
                int num = base.Params.Output.Count - 1;
                for (int i = 0; i <= num; i++)
                {
                    if (string.IsNullOrWhiteSpace(base.Params.Output[i].NickName) || base.Params.Output[i].NickName == "{ }")
                    {
                        base.Params.Output[i].NickName = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", base.Params.Output);
                    }
                    base.Params.Output[i].Name = $"General Data {base.Params.Output[i].NickName}";
                    base.Params.Output[i].Description = "Output data with a tag";
                    base.Params.Output[i].Access = GH_ParamAccess.tree;
                    base.Params.Output[i].Optional = true;
                }
            }
        }
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            this.VariableParameterMaintenance();
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "The label for your data", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("General Data A", "A", "Output data with a tag", GH_ParamAccess.tree);
        }
        private bool MatchOutput()
        {
            var keys = this.tagChannel?.Keys.ToList();
            int pathCount = Math.Max(1, keys?.Count ?? 0);
            bool changed = false;

            if(base.Params.Output.Count == pathCount)
            {
                for(int i = 0; keys != null && i < keys.Count; i++)
                {
                    if(base.Params.Output[i].NickName != keys[i])
                    {
                        base.Params.Output[i].NickName = keys[i];
                        changed = true;
                    }
                }
                VariableParameterMaintenance();
                if(changed)
                    base.Params.OnParametersChanged();
                return false;
            }
            RecordUndoEvent("Exlode Channel");
            if(base.Params.Output.Count < pathCount)
            {
                while(base.Params.Output.Count < pathCount)
                {
                    var new_Param = CreateParameter(GH_ParameterSide.Output, base.Params.Output.Count);
                    base.Params.RegisterOutputParam(new_Param);
                }
            }
            else if(base.Params.Output.Count > pathCount)
            {
                while(base.Params.Output.Count > pathCount)
                {
                    base.Params.UnregisterOutputParameter(base.Params.Output[checked(base.Params.Output.Count - 1)]);
                }
            }
            for(int i = 0; keys != null && i < keys.Count; i++)
            {
                base.Params.Output[i].NickName = keys[i];
            }
            base.Params.OnParametersChanged();
            VariableParameterMaintenance();
            ExpireSolution(true);
            return true;
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var tag = "";
            if (!DA.GetData("Tag", ref tag))
            {
                return;
            }
            DA.DisableGapLogic();
            if (DA.Iteration > 0)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }
            this.SingletonTag = tag;

            if (!IsPrimaryInstance())
            {
                var inValid = new DataTree<int>();
                inValid.Add(-1);
                DA.SetDataTree(0, inValid);
            }
            this.UpdateProcessOutput(tag);
            var doc = this.OnPingDocument();
            var target_Component = doc.Objects.OfType<GH_TagChannel_Abstract>()
            .FirstOrDefault(x => x.ChannelType == RemoteType.Input && x.SingletonTag == this.SingletonTag);

            if (target_Component == null || target_Component.tagChannel == null || !target_Component.tagChannel.HasValidChannel())
            {
                var inValid = new DataTree<int>();
                inValid.Add(-1);
                DA.SetDataTree(0, inValid);
                return;
            }
            
            this.tagChannel = target_Component.tagChannel;
            if(this.MatchOutput())
                return;


            checked
            {
                int num = base.Params.Output.Count - 1;
                for (int i = 0; i <= num; i++)
                {
                    var key = ChannelKey(base.Params.Output[i], i);
                    if (!this.tagChannel.TryGetValue(key, out var dataTree) || dataTree == null)
                    {
                        DA.SetDataTree(i, new DataTree<IGH_Goo>());
                        continue;
                    }
                    DA.SetDataTree(i, dataTree);
                }
            }
        }
    }
}

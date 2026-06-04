using System;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Control.Timeline;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Stores general data under named input nicknames inside a tag channel. A tag acts as the channel label, and each variable input nickname becomes a data key that can be read by matching Tag Channel Output components. Inputs include Tag and General Data A.
    /// </summary>
    public class GH_TagChannel_IN : GH_TagChannel_Abstract, IGH_VariableParameterComponent
    {
        public GH_TagChannel_IN():base("Tag Channel Input", "In_Tag", "Connect data without Grasshopper wires, and label the data with a tag"){}
        public override RemoteType ChannelType => RemoteType.Input;

        public override Guid ComponentGuid => new Guid("0aa0d4ab-a76c-4a8d-ab81-c90ed111415c");

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            // Keep the first input reserved for the Tag. New data inputs can only be inserted after it.
            return side == GH_ParameterSide.Input && index > 0;
        }
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return this.CanInsertParameter(side, index);
        }
        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            // Do not allow removing the Tag input.
            return side == GH_ParameterSide.Input && index > 0;
        }
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return this.CanRemoveParameter(side, index);
        }
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            // IMPORTANT: Do not call Params.RegisterInputParam here.
            // Grasshopper will register the returned parameter automatically.
            return new Param_GenericObject
            {
                Name = "General Data",
                NickName = string.Empty,
                Description = "Input data with a tag",
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
            var sameTag = doc.Objects
            .OfType<GH_TagChannel_Abstract>()
            .Where(x => x.ChannelType == this.ChannelType)
            .Where(x => x.SingletonTag == this.SingletonTag)
            .OrderBy(x => x.InstanceGuid).ToList();
            return sameTag.Count == 1;
        }

        public void VariableParameterMaintenance()
        {
            base.Params.Input[0].Optional = false;
            base.Params.Input[0].Access = GH_ParamAccess.item;
            base.Params.Input[1].Optional = false;
            base.Params.Input[1].Access = GH_ParamAccess.tree;

            checked
            {
                int num = base.Params.Input.Count - 1;
                for (int i = 1; i <= num; i++)
                {
                    if(string.IsNullOrWhiteSpace(base.Params.Input[i].NickName))
                    {
                        base.Params.Input[i].NickName = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", base.Params.Input);
                    }
                    base.Params.Input[i].Name = $"General Data {base.Params.Input[i].NickName}";
                    base.Params.Input[i].Description = "Input data with a tag";
                    base.Params.Input[i].Access = GH_ParamAccess.tree;
                    base.Params.Input[i].Optional = true;
                }
            }
        }
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            this.VariableParameterMaintenance();
            ExpireSolution(true);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag", "Tag", "The label for your data", GH_ParamAccess.item);
            pManager.AddGenericParameter("General Data A", "A", "Input data with a tag", GH_ParamAccess.tree);
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
                foreach (var outComp in doc.Objects.OfType<GH_TagChannel_Abstract>().Where(
                    x => (x.ChannelType == RemoteType.Output || x.ChannelType == RemoteType.Process) && x.SingletonTag == this.SingletonTag)
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
            DA.GetData("Tag", ref tag);
            var oldTag = this.SingletonTag;
            DA.DisableGapLogic();
            if(DA.Iteration > 0)
            {
                return;
            }
            this.tagChannel = new TagChannel<IGH_Goo>(tag);
            this.SingletonTag = tag;
            checked
            {
                int num = base.Params.Input.Count - 1;
                for(int i = 1; i <= num; i++)
                {
                    GH_Structure<IGH_Goo> tree = null;
                    if(DA.GetDataTree(i, out tree))
                    {
                        var dataTree = new DataTree<IGH_Goo>();
                        if(DataUtil.GH_Structure2GHDataTreeIGH_Goo(tree, ref dataTree))
                            this.tagChannel[ChannelKey(base.Params.Input[i], i - 1)] = dataTree;
                    }
                }
            }
            UpdateRemoteOutput();
            if(oldTag != tag)
            {
                UpdateProcessOutput(oldTag);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.UI.Theme;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Splits a list or data tree into dynamically generated outputs. The input data provides branches or items, and the optional path selects a branch before outputs are created for downstream components. Inputs include DataList and Path. Outputs include Item 0.
    /// </summary>
    public class GH_MatchList : GH_Component, IGH_VariableParameterComponent
    {
        public GH_MatchList():base("Match List output", "Match List", "Create one output for each branch or item in an input list.", "Woodpecker", "Util"){}
        public override Guid ComponentGuid => new Guid("3446f404-891c-4bb2-885a-3407fc5198e1");

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
            // Keep at least one output parameter on the component.
            return side == GH_ParameterSide.Output && base.Params.Output.Count > 1;
        }
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return this.CanRemoveParameter(side, index);
        }
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject
            {
                Name = "Item",
                NickName = string.Empty,
                Description = "an item in the list",
                Optional = true,
                Access = GH_ParamAccess.item
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
        public void VariableParameterMaintenance()
        {
            checked
            {
                int num = base.Params.Output.Count - 1;
                for (int i = 0; i <= num; i++)
                {
                    base.Params.Output[i].Name = $"Item {i}";
                    base.Params.Output[i].NickName = $"+{i}";
                    base.Params.Output[i].Description = "An item in the list";
                    base.Params.Output[i].Access = GH_ParamAccess.item;
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
            pManager.AddGenericParameter("DataList", "List", "input list of data", GH_ParamAccess.tree);
            pManager.AddPathParameter("Path", "P", "Optional data-tree path used to select a branch.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Item 0", "0", "an item in the list", GH_ParamAccess.item);
        }
        private List<IGH_Goo> _values = new List<IGH_Goo>();
        private bool _resizeScheduled = false;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree<IGH_Goo>("DataList", out var valueTree);
            var path = new GH_Path(0);
            DA.GetData("Path", ref path);

            if(path == new GH_Path(0))
            {
                _values = valueTree.Branches.First();
            }
            else
            {
                _values = new List<IGH_Goo>();
                var valueDataTree = new DataTree<IGH_Goo>();
                DataUtil.GH_Structure2GHDataTreeIGH_Goo(valueTree, ref valueDataTree);
                _values = valueDataTree.Branch(path);

                if(_values == null)
                {
                    _values = valueTree.Branches.First();
                }
            }

            

            ScheduleOutputMatch();

            checked
            {
                int num = base.Params.Output.Count - 1;
                for (int i = 0; i <= num; i++)
                {
                    if (i < _values.Count)
                        DA.SetData(i, _values[i]);
                    else
                        DA.SetData(i, null);
                }
            }
        }
        private void ScheduleOutputMatch()
        {
            int pathCount = Math.Max(1, _values.Count);
            if(base.Params.Output.Count == pathCount)
            {
                return;
            }
            if (_resizeScheduled)
                return;

            _resizeScheduled = true;
            var doc = OnPingDocument();
            if (doc == null)
            {
                MatchOutput(pathCount);
                _resizeScheduled = false;
                return;
            }

            doc.ScheduleSolution(1, scheduledDoc =>
            {
                _resizeScheduled = false;
                MatchOutput(Math.Max(1, _values.Count));
                ExpireSolution(false);
            });
        }
        private void MatchOutput(int pathCount)
        {
            RecordUndoEvent("Match List Outputs");
            if(base.Params.Output.Count < pathCount)
            {
                while(base.Params.Output.Count < pathCount)
                {
                    var new_Params = CreateParameter(GH_ParameterSide.Output, base.Params.Output.Count);
                    base.Params.RegisterOutputParam(new_Params);
                }
            }
            else if(base.Params.Output.Count > pathCount)
            {
                while (base.Params.Output.Count > pathCount)
                {
                    base.Params.UnregisterOutputParameter(base.Params.Output[checked(base.Params.Output.Count - 1)]);
                }
            }
            VariableParameterMaintenance();
            Params.OnParametersChanged();
            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
        }
    }
}

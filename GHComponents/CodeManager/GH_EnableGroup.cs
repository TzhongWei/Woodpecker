using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Enable or disable components by provide the group name. Inputs include Enable and GroupName. Outputs include Result.
    /// </summary>
    public class GH_EnableGroup : GH_CodeManagerAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public GH_EnableGroup():base("Enable Group", "EGroup", "Enable or disable components by provide the group name"){}
        public override Guid ComponentGuid => new Guid("8cfab4ad-79de-4c06-9f34-e01ba1f10bea");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Enable", "E", "Enable or disable a group of components by group name", GH_ParamAccess.item);
            pManager.AddTextParameter("GroupName", "Name", "The nickname of the component group", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "The result of this component", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            setdoc();
            var _enable = false;
            string _name = "";
            DA.GetData("Enable", ref _enable);
            DA.GetData("GroupName", ref _name);
            var tempCodeManager = new CodeManagerUtil(this._doc, _name, ManageType.Group);
            if(this._codeManagerUtil == null || tempCodeManager != _codeManagerUtil)
                this._codeManagerUtil = tempCodeManager;
            
            this._result = this._codeManagerUtil.EnableToggle(_enable);

            this._reportMessage = _result ? $"Active Successfully, current {_name} status is changed" : $"Active failed, current {_name} cannot be found"; 

            DA.SetData("Result", this._reportMessage);
        }
    }
}
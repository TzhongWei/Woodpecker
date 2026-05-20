using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Display or hidden previous geometry by a component type by provide its' name or nickname. Inputs include Display and ComponentName. Outputs include Result.
    /// </summary>
    public class GH_DisplayName : GH_CodeManagerAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_DisplayName():base("Display Name", "DName", "Display or hidden previous geometry by a component type by provide its' name or nickname"){}
        public override Guid ComponentGuid => new Guid("59dc82cd-aa0b-480a-a5d1-ae0fb4a80b53");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Display", "D", "Display or hidden previous geometry by a group of components by type's name or nickname", GH_ParamAccess.item);
            pManager.AddTextParameter("ComponentName", "Name", "The nickname or name of the components", GH_ParamAccess.item);
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
            DA.GetData("Display", ref _enable);
            DA.GetData("ComponentName", ref _name);
            var tempCodeManager = new CodeManagerUtil(this._doc, _name, ManageType.Component);
            if(this._codeManagerUtil == null || tempCodeManager != _codeManagerUtil)
                this._codeManagerUtil = tempCodeManager;
            
            this._result = this._codeManagerUtil.DisplayToggle(_enable);

            this._reportMessage = _result ? $"Active Successfully, current {_name} status is changed" : $"Active failed, current {_name} cannot be found"; 

            DA.SetData("Result", this._reportMessage);
        }
    }
}
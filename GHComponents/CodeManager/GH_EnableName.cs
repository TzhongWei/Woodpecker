using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Enable or disable a component type by provide its' name or nickname. Inputs include Enable and ComponentName. Outputs include Result.
    /// </summary>
    public class GH_EnableName : GH_CodeManagerAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public GH_EnableName():base("Enable Name", "EName", "Enable or disable a component type by provide its' name or nickname"){}
        public override Guid ComponentGuid => new Guid("3062621c-80fd-40f8-a610-ef0549c71d4b");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Enable", "E", "Enable or disable a group of components by type's name or nickname", GH_ParamAccess.item);
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
            DA.GetData("Enable", ref _enable);
            DA.GetData("ComponentName", ref _name);
            var tempCodeManager = new CodeManagerUtil(this._doc, _name, ManageType.Component);
            if(this._codeManagerUtil == null || tempCodeManager != _codeManagerUtil)
                this._codeManagerUtil = tempCodeManager;
            
            this._result = this._codeManagerUtil.EnableToggle(_enable);

            this._reportMessage = _result ? $"Active Successfully, current {_name} status is changed" : $"Active failed, current {_name} cannot be found"; 

            DA.SetData("Result", this._reportMessage);
        }
    }
}
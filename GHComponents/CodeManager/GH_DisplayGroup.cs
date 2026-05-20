using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Display or hidden previous geometry by components by provide the group name. Inputs include Display and GroupName. Outputs include Result.
    /// </summary>
    public class GH_DisplayGroup : GH_CodeManagerAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_DisplayGroup():base("Display Group", "DGroup", "Display or hidden previous geometry by components by provide the group name"){}
        public override Guid ComponentGuid => new Guid("0145176f-b145-4e3d-9aa9-39dd62af345f");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Display", "D", "Display or hidden previous geometry by a group of components by group name", GH_ParamAccess.item);
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
            DA.GetData("Display", ref _enable);
            DA.GetData("GroupName", ref _name);
            var tempCodeManager = new CodeManagerUtil(this._doc, _name, ManageType.Group);
            if(this._codeManagerUtil == null || tempCodeManager != _codeManagerUtil)
                this._codeManagerUtil = tempCodeManager;
            
            this._result = this._codeManagerUtil.DisplayToggle(_enable);

            this._reportMessage = _result ? $"Active Successfully, current {_name} status is changed" : $"Active failed, current {_name} cannot be found"; 

            DA.SetData("Result", this._reportMessage);
        }
    }
}
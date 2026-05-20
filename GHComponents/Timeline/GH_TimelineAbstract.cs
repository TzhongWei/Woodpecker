using System.Configuration;
using System.Data;
using System.Windows.Forms;
using Grasshopper.Kernel;
using System;
using GH_IO.Serialization;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Timeline Abstract component.
    /// </summary>
    public abstract class GH_TimelineAbstract : GH_Component
    {
        public GH_TimelineAbstract(string componentName, string nickname, string description): 
        base(componentName, nickname, description, "Woodpecker", "Timeline")
        {
        }
        protected abstract string ShowTimeSetupDescription();
        private bool _toggle = true;
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Display Timeline", ToogleShowTimelineSetup, true, _toggle);
        }
        protected virtual void ToogleShowTimelineSetup(object sender, EventArgs e)
        {
            _toggle = !_toggle;
            ExpireSolution(true);
        }
        protected virtual void MessageSetup()
        {
            if(_toggle)
              this.Message = ShowTimeSetupDescription();
            else
                this.Message = "";
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ShowTimeDescription", _toggle);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("ShowTimeDescription", ref _toggle);
            return base.Read(reader);
        }
    }
}
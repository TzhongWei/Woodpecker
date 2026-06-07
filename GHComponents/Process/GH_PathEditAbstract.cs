using System.Collections.Generic;
using System.Drawing;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    /// <summary>
    /// Path Edit Abstract component.
    /// </summary>
    public abstract class GH_PathEditAbstract : GH_Component
    {
        public GH_PathEditAbstract(string Name, string NickName, string Description) : base(Name, NickName, Description, "Woodpecker", "Process")
        {

        }
        protected int _option = 0;
        protected List<Color> optionColours = new List<Color> { Color.FromArgb(70, 255, 81, 81), Color.FromArgb(70, 220, 255, 81), Color.FromArgb(70, 81, 101, 255) }; // rgba(255, 81, 81, 0.7) rgba(220, 255, 81, 0.7) rgba(81, 101, 255, 0.7)
        public override void CreateAttributes()
        {
            this.m_attributes = new ButtonUIAttributesState(this, new List<string> { "JointCurve", "SplitCurve", "OnlyAddedCurves" }, Switch, optionColours, "output curves options", _option);
        }
        
        protected void Switch()
        {
            _option = (_option + 1) % 3;
            (this.Attributes as ButtonUIAttributesState).UpdateSelectedIndex(_option);
            this.Attributes?.ExpireLayout();
            this.OnDisplayExpired(true);
            this.ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("option", _option);
            return base.Write(writer);
        }
        
        public override bool Read(GH_IReader reader)
        {
            bool result = base.Read(reader);
            if (result &= reader.TryGetInt32("option", ref _option))
            {
                (this.Attributes as ButtonUIAttributesState).UpdateSelectedIndex(_option);
                this.Attributes?.ExpireLayout();
                this.OnDisplayExpired(true);
                this.ExpireSolution(true);
            }
            return result;
        }
    }
}
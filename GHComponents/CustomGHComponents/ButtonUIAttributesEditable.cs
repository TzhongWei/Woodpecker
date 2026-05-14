using System;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ButtonUIAttributesEditable : ButtonUIAttributes
    {
        private Action _showEditor;
        public ButtonUIAttributesEditable(GH_Component owner, string displayText, Action clickHandle, Action showEditor, string spacerText = ""):
        base(owner, displayText, clickHandle, spacerText)
        {
            this._showEditor = showEditor;
        }
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this._showEditor();
                return GH_ObjectResponse.Handled;
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}
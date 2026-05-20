using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;


namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ButtonUIAttributesStateEditable : ButtonUIAttributesState
    {
        private Action _showEditor;
        private readonly Func<bool> _highlightBoundary;
        public ButtonUIAttributesStateEditable(GH_Component owner, List<string> displayText, Action clickHandle, Action showEditor, ButtonColourStateSetting colourState, string spacerText = "", int initialstate = 0, Func<bool> highlightBoundary = null):
        base(owner, displayText, clickHandle, colourState, spacerText, initialstate)
        {
            this._showEditor = showEditor;
            this._highlightBoundary = highlightBoundary;
        }
        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if(channel != GH_CanvasChannel.Objects || _highlightBoundary == null || !_highlightBoundary())
                return;

            var bounds = Bounds;
            bounds.Inflate(6, 6);
            using (var fill = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(34, 255, 92, 36)))
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(230, 255, 76, 24), 3.0f))
            using (var path = AttributeUtil.RoundedRect(bounds, 8))
            {
                graphics.FillPath(fill, path);
                graphics.DrawPath(pen, path);
            }
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

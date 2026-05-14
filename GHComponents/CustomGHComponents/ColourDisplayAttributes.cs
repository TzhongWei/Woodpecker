using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Woodpecker.Animation.Util.IO;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ColourDisplayAttributes : GH_ComponentAttributes
    {
        private RectangleF _HeaderBounds;
        private Dictionary<string, List<Color>> _colourCode = new Dictionary<string, List<Color>>();
        public ColourDisplayAttributes(IGH_Component owner, Dictionary<string, List<Color>> ColourCode) : base(owner)
        {
            this._colourCode = ColourCode;
        }
        public void UpdateColourCode(Dictionary<string, List<Color>> ColourCode)
        {
            this._colourCode = ColourCode;
        }
        protected override void Layout()
        {
            base.Layout();
            _HeaderBounds = Bounds;
            var owner = Owner as GH_ColourCodePanel;
            if (owner == null) return;

            int maxTextWidth = 100;
            int maxSwatches = 1;

            foreach (var row in this._colourCode)
            {
                maxTextWidth = Math.Max(maxTextWidth, GH_FontServer.StringWidth(row.Key, GH_FontServer.Standard));
                maxSwatches = Math.Max(maxSwatches, row.Value.Count);
            }
            int swatchSize = 18;
            int gap = 5;
            int rowHeight = 24;
            int padding = 8; // space around the content in the panel
            // | padding| textwidth | maxSwatches * swatches | 8 |
            int panelWidth = padding + maxTextWidth + maxSwatches * (swatchSize + gap) + 8;
            // ---
            // padding
            // owner.ColourCodeDic.Count * rowHeight
            // ---
            int panelHeight = padding + _colourCode.Count * rowHeight;

            float totalWidth = Math.Max(_HeaderBounds.Width, panelWidth);
            float totalHeight = _HeaderBounds.Height + 2f + panelHeight;

            Bounds = new RectangleF(_HeaderBounds.X, _HeaderBounds.Y, totalWidth, totalHeight);

            //LayoutInputParams(owner, Bounds);  <- error format
            //LayoutOutputParams(owner, Bounds); <- error format
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            // Render the headerBounds, render the whole bounds
            base.Render(canvas, graphics, channel);

            // Panel below the header
            float padding = 30f;  //10f. Margin for all
            float rowHeight = 24f;
            float swatchSize = 16f;
            float gap = 4f;

            float y = Bounds.Top + padding;

            foreach (var row in this._colourCode)
            {
                //draw text
                var textRect = new RectangleF(Bounds.Left + 5f, y, 120, rowHeight); //Margin for text on left
                graphics.DrawString(
                    row.Key,
                    GH_FontServer.Standard,
                    Brushes.Black,
                    textRect,
                    new StringFormat { LineAlignment = StringAlignment.Center }
                );

                //draw swatches
                float x = Bounds.Left + padding + 80f;  //130f

                foreach (var c in row.Value)
                {
                    var swatchRect = new RectangleF(x, y + 4f, swatchSize, swatchSize);


                    // Create a fillet rectangle path
                    using (var path = AttributeUtil.RoundedRect(swatchRect, 4f))
                    {
                        var oldClip = graphics.Clip;
                        graphics.SetClip(path);
                        //draw a checkerboard pattern for transparency
                        ColourCodeUtil.DrawCheckerboard(graphics, swatchRect, 4f);

                        using (var brush = new SolidBrush(c))
                            graphics.FillRectangle(brush, swatchRect);

                        graphics.Clip = oldClip;
                    }

                    using(var border = AttributeUtil.RoundedRect(swatchRect, 4f))
                    {
                        graphics.DrawPath(Pens.Black, border);
                    }
                    x += swatchSize + gap;
                }

                y += rowHeight;
            }
        }
    }
}

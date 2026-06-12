using System.Drawing;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Woodpecker.Animation.Util.IO;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ColourDisplayAttributes : GH_ComponentAttributes
    {
        private const float Padding = 8f;
        private const float HeaderHeight = 24f;
        private const float ParameterHeight = 20f;
        private const float ConnectorOffset = 8f;

        private RectangleF _titleBounds;
        private RectangleF _parameterBounds;
        private Dictionary<string, List<Color>> _colourCode = new Dictionary<string, List<Color>>();

        public ColourDisplayAttributes(IGH_Component owner, Dictionary<string, List<Color>> ColourCode) : base(owner)
        {
            _colourCode = CloneColourCode(ColourCode);
        }

        public bool UpdateColourCode(Dictionary<string, List<Color>> ColourCode)
        {
            ColourCode = ColourCode == null ? new Dictionary<string, List<Color>>() : ColourCode;
            if (ColourCodesEqual(_colourCode, ColourCode))
                return false;

            _colourCode = CloneColourCode(ColourCode);
            return true;
        }

        protected override void Layout()
        {
            base.Layout();
            var originalBounds = Bounds;

            int maxTextWidth = 100;
            int maxSwatches = 1;

            foreach (var row in _colourCode)
            {
                maxTextWidth = Math.Max(maxTextWidth, GH_FontServer.StringWidth(row.Key, GH_FontServer.Standard));
                maxSwatches = Math.Max(maxSwatches, row.Value?.Count ?? 0);
            }

            int swatchSize = 18;
            int gap = 5;
            int rowHeight = 24;
            int panelWidth = (int)Padding + maxTextWidth + gap +
                maxSwatches * (swatchSize + gap) + (int)Padding;
            int panelHeight = _colourCode.Count == 0
                ? 0
                : (int)Padding * 2 + _colourCode.Count * rowHeight;

            float titleWidth = GH_FontServer.StringWidth(
                Owner.NickName,
                GH_FontServer.StandardAdjusted) + Padding * 2f;
            float parameterWidth = Owner.Params.Input.Count == 0
                ? 0f
                : GH_FontServer.StringWidth(
                    Owner.Params.Input[0].NickName,
                    GH_FontServer.StandardAdjusted) + Padding * 2f;

            float totalWidth = Math.Max(
                Math.Max(originalBounds.Width, panelWidth),
                Math.Max(titleWidth, parameterWidth));
            float totalHeight = HeaderHeight + ParameterHeight + panelHeight;

            Bounds = new RectangleF(
                originalBounds.X,
                originalBounds.Y,
                totalWidth,
                totalHeight);

            _titleBounds = new RectangleF(
                Bounds.Left,
                Bounds.Top,
                Bounds.Width,
                HeaderHeight);

            _parameterBounds = new RectangleF(
                Bounds.Left + Padding,
                _titleBounds.Bottom,
                Bounds.Width - Padding * 2f,
                ParameterHeight);

            LayoutParameters();
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Wires)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            if (channel != GH_CanvasChannel.Objects)
                return;

            var visibleBounds = Bounds;
            if (!canvas.Viewport.IsVisible(ref visibleBounds, 10f))
                return;

            var palette = GH_Palette.Normal;
            if (Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning)
                palette = GH_Palette.Warning;
            else if (Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error)
                palette = GH_Palette.Error;

            using (var capsule = GH_Capsule.CreateCapsule(Bounds, palette))
            {
                capsule.Render(graphics, Selected, Owner.Locked, Owner.Hidden);
            }

            var impliedStyle = GH_CapsuleRenderEngine.GetImpliedStyle(palette, this);
            RenderComponentParameters(canvas, graphics, Owner, impliedStyle);

            if (GH_Canvas.ZoomFadeLow <= 0)
                return;

            canvas.SetSmartTextRenderingHint();
            using (var titleBrush = new SolidBrush(Color.FromArgb(GH_Canvas.ZoomFadeLow, impliedStyle.Text)))
            {
                graphics.DrawString(
                    Owner.NickName,
                    GH_FontServer.StandardAdjusted,
                    titleBrush,
                    _titleBounds,
                    GH_TextRenderingConstants.CenterCenter);
            }

            float rowHeight = 24f;
            float swatchSize = 16f;
            float gap = 4f;
            float labelWidth = Math.Max(100f, _colourCode.Keys
                .Select(key => (float)GH_FontServer.StringWidth(key, GH_FontServer.StandardAdjusted))
                .DefaultIfEmpty(100f)
                .Max());

            float y = _parameterBounds.Bottom + Padding;
            var rowFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            foreach (var row in _colourCode)
            {
                var textRect = new RectangleF(Bounds.Left + Padding, y, labelWidth, rowHeight);
                graphics.DrawString(
                    row.Key,
                    GH_FontServer.StandardAdjusted,
                    Brushes.Black,
                    textRect,
                    rowFormat);

                float x = textRect.Right + gap;

                foreach (var colour in row.Value ?? Enumerable.Empty<Color>())
                {
                    var swatchRect = new RectangleF(x, y + 4f, swatchSize, swatchSize);

                    using (var path = AttributeUtil.RoundedRect(swatchRect, 4f))
                    {
                        var graphicsState = graphics.Save();
                        graphics.SetClip(path);
                        ColourCodeUtil.DrawCheckerboard(graphics, swatchRect, 4f);

                        using (var brush = new SolidBrush(colour))
                            graphics.FillRectangle(brush, swatchRect);

                        graphics.Restore(graphicsState);
                    }

                    using (var border = AttributeUtil.RoundedRect(swatchRect, 4f))
                    {
                        graphics.DrawPath(Pens.Black, border);
                    }

                    x += swatchSize + gap;
                }

                y += rowHeight;
            }
        }

        private void LayoutParameters()
        {
            if (Owner.Params.Input.Count == 0)
                return;

            float rowHeight = _parameterBounds.Height /
                Owner.Params.Input.Count;

            for (int index = 0; index < Owner.Params.Input.Count; index++)
            {
                var parameter = Owner.Params.Input[index];
                float rowTop = _parameterBounds.Top + index * rowHeight;

                parameter.Attributes.Pivot = new PointF(
                    Bounds.Left - ConnectorOffset,
                    rowTop + rowHeight * 0.5f);

                parameter.Attributes.Bounds = new RectangleF(
                    Bounds.Left,
                    rowTop,
                    Math.Max(
                        _parameterBounds.Width,
                        GH_FontServer.StringWidth(
                            parameter.NickName,
                            GH_FontServer.StandardAdjusted) + Padding * 2f),
                    rowHeight);
            }
        }

        private static bool ColourCodesEqual(
            Dictionary<string, List<Color>> first,
            Dictionary<string, List<Color>> second)
        {
            if (ReferenceEquals(first, second))
                return true;
            if (first == null || second == null || first.Count != second.Count)
                return false;

            foreach (var pair in first)
            {
                if (!second.TryGetValue(pair.Key, out var otherColours))
                    return false;

                var colours = pair.Value;
                if (ReferenceEquals(colours, otherColours))
                    continue;
                if (colours == null || otherColours == null || colours.Count != otherColours.Count)
                    return false;

                for (int i = 0; i < colours.Count; i++)
                {
                    if (colours[i].ToArgb() != otherColours[i].ToArgb())
                        return false;
                }
            }

            return true;
        }

        private static Dictionary<string, List<Color>> CloneColourCode(
            Dictionary<string, List<Color>> source)
        {
            if (source == null)
                return new Dictionary<string, List<Color>>();

            return source.ToDictionary(
                pair => pair.Key,
                pair => pair.Value?.ToList() ?? new List<Color>());
        }
    }
}

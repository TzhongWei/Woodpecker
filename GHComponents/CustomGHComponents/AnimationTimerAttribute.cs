using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Expressions;
using Grasshopper.Kernel.Undo;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class AnimationTimerAttributes : ResizableAttributes<GH_AnimationTimer>
    {
        private class SliderDragUndo : GH_ObjectUndoAction
        {
            private decimal _value;

            public override bool ExpiresSolution { get; }

            public SliderDragUndo(GH_AnimationTimer owner) : base(owner.InstanceGuid)
            {
                _value = owner.Slider.Value;
                ExpiresSolution = true;
            }

            protected override void Object_Undo(GH_Document doc, IGH_DocumentObject obj)
            {
                if (obj is GH_AnimationTimer timer)
                {
                    var value = timer.Slider.Value;
                    timer.Slider.Value = _value;
                    _value = value;
                }
            }

            protected override void Object_Redo(GH_Document doc, IGH_DocumentObject obj)
            {
                Object_Undo(doc, obj);
            }
        }

        private const int HeaderHeight = 22;
        private const int SliderHeight = 24;
        private const int ButtonHeight = 22;
        private const int ParamHeight = 20;
        private const int Padding = 6;

        private int _dragMode;
        private bool _resumeAfterDrag;
        private string _cachedName;
        private Rectangle _tagBounds;
        private Rectangle _sliderBounds;
        private Rectangle _buttonBounds;
        private Rectangle _toStartBounds;
        private Rectangle _playBounds;
        private Rectangle _toEndBounds;

        protected override Size MinimumSize => new Size(170, HeaderHeight + SliderHeight + ButtonHeight + Padding * 4);
        protected override Size MaximumSize => new Size(5000, HeaderHeight + SliderHeight + ButtonHeight + Padding * 4);
        protected override Padding SizingBorders => new Padding(0, 0, 6, 0);

        public override bool HasInputGrip => true;
        public override bool HasOutputGrip => true;
        public override bool TooltipEnabled => true;

        public AnimationTimerAttributes(GH_AnimationTimer owner) : base(owner)
        {
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Wires)
            {
                foreach (var param in Owner.Params)
                    param.Attributes?.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
                return;
            }

            if (channel == GH_CanvasChannel.Objects)
            {
                RenderComponentCapsule(canvas, graphics, true, false, true, true, true, true);
                return;
            }


            var name = Owner.SingletonTag;
            if (string.IsNullOrWhiteSpace(name))
                name = "Global_T";

            if (_cachedName == null || !name.Equals(_cachedName, StringComparison.Ordinal))
            {
                _cachedName = name;
                ExpireLayout();
                Layout();
            }

            var bounds = Bounds;
            if (!canvas.Viewport.IsVisible(ref bounds, 10f))
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
            /// Render inputs and outputs texts
            RenderComponentParameters(canvas, graphics, base.Owner, impliedStyle); 
            
            // Render Input and output
            // switch (channel)
            // {
            //     case GH_CanvasChannel.Wires:
            //         foreach (var param in Owner.Params)
            //         {
            //             param.Attributes.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
            //         }
            //         break;
            //     case GH_CanvasChannel.Objects:
            //         RenderComponentCapsule(canvas, graphics, true, false, true, true, true, true);
            //         break;
            // }


            if (GH_Canvas.ZoomFadeLow <= 0)
                return;

            canvas.SetSmartTextRenderingHint();


            /// Draw slider
            graphics.DrawString(name, GH_FontServer.StandardAdjusted, AnimationTimerAttributesColourSetting.AnnotationTextDark, _tagBounds, GH_TextRenderingConstants.CenterCenter);
            var sliderOutterBounds = _sliderBounds;
            var sliderPath = AttributeUtil.RoundedRect(sliderOutterBounds, 2);
            var sliderPen = new Pen(AnimationTimerAttributesColourSetting.SliderBorderColour, 0.8f);

            graphics.FillPath(AnimationTimerAttributesColourSetting.SliderColor, sliderPath);
            var overlay = AttributeUtil.RoundedRect(sliderOutterBounds, 2, true);
            graphics.FillPath(new SolidBrush(Color.FromArgb(40, 255, 255, 255)), overlay);
            graphics.DrawPath(sliderPen, sliderPath);

            Owner.Slider.FormatMask = "{0}";
            Owner.Slider.Render(graphics);


            /// Setting button
            ButtonRender(graphics, _toStartBounds, "<<", _mouseOverToStart, _mouseDownToStart);
            ButtonRender(graphics, _playBounds, Owner.PlayState ? "Stop" : "Start", _mouseOverPlay, _mouseDownPlay);
            ButtonRender(graphics, _toEndBounds, ">>", _mouseOverToEnd, _mouseDownToEnd);

            // var style = GH_CapsuleRenderEngine.GetImpliedStyle(palette, this);
            // using (var textBrush = new SolidBrush(Color.FromArgb(GH_Canvas.ZoomFadeLow, style.Text)))
            // using (var buttonBrush = new SolidBrush(Owner.PlayState ? Color.FromArgb(220, 255, 214, 150) : Color.FromArgb(220, 225, 225, 225)))
            // using (var jumpBrush = new SolidBrush(Color.FromArgb(220, 235, 235, 235)))
            // using (var buttonBorder = new Pen(_mouseOverButton ? Color.OrangeRed : Color.FromArgb(120, Color.Black), 1f))
            // {
            //     graphics.DrawString(name, GH_FontServer.StandardAdjusted, textBrush, _tagBounds, GH_TextRenderingConstants.CenterCenter);

            //     Owner.Slider.FormatMask = "{0}";
            //     Owner.Slider.Render(graphics);
            //     graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //     graphics.FillRectangle(jumpBrush, _toStartBounds);
            //     graphics.DrawRectangle(buttonBorder, _toStartBounds);
            //     graphics.DrawString("<<", GH_FontServer.StandardAdjusted, textBrush, _toStartBounds, GH_TextRenderingConstants.CenterCenter);

            //     graphics.FillRectangle(buttonBrush, _playBounds);
            //     graphics.DrawRectangle(buttonBorder, _playBounds);
            //     var label = Owner.PlayState ? "Stop" : "Start";
            //     graphics.DrawString(label, GH_FontServer.StandardAdjusted, textBrush, _playBounds, GH_TextRenderingConstants.CenterCenter);

            //     graphics.FillRectangle(jumpBrush, _toEndBounds);
            //     graphics.DrawRectangle(buttonBorder, _toEndBounds);
            //     graphics.DrawString(">>", GH_FontServer.StandardAdjusted, textBrush, _toEndBounds, GH_TextRenderingConstants.CenterCenter);
            // }
        }

        protected override void Layout()
        {
            //base.Layout();
            SliderAndButtonLayout();
            // FixParamLayout();
            ParameterLayout();
        }
        private void SliderAndButtonLayout()
        {
            var width = Math.Max(Bounds.Width, MinimumSize.Width);
            var _maxParamsCount = Math.Max(Owner.Params.Input.Count, Owner.Params.Output.Count);
            Bounds = new RectangleF(Pivot.X, Pivot.Y, width, MinimumSize.Height + _maxParamsCount * ParamHeight);
            var left = Convert.ToInt32(Bounds.Left) + Padding;
            var top = Convert.ToInt32(Bounds.Top) + Padding;
            var innerWidth = Convert.ToInt32(Bounds.Width) - Padding * 2;

            _tagBounds = new Rectangle(
                left,
                top,
                innerWidth,
                HeaderHeight);


            var _sliderBottom = _tagBounds.Bottom + _maxParamsCount * (ParamHeight + Padding);

            _sliderBounds = new Rectangle(
                left,
                _sliderBottom,
                innerWidth,
                SliderHeight
            );
            _buttonBounds = new Rectangle(
                left,
                _sliderBounds.Bottom + Padding,
                innerWidth,
                ButtonHeight
            );

            var jumpWidth = Math.Min(44, Math.Max(30, innerWidth / 5));
            _toStartBounds = new Rectangle(_buttonBounds.Left, _buttonBounds.Top, jumpWidth, _buttonBounds.Height);
            _toEndBounds = new Rectangle(_buttonBounds.Right - jumpWidth, _buttonBounds.Top, jumpWidth, _buttonBounds.Height);
            _playBounds = new Rectangle(_toStartBounds.Right + Padding, _buttonBounds.Top, innerWidth - jumpWidth * 2 - Padding * 2, _buttonBounds.Height);

            Owner.Slider.Font = GH_FontServer.StandardAdjusted;
            Owner.Slider.DrawControlBorder = false;
            Owner.Slider.DrawControlShadows = false;
            Owner.Slider.DrawControlBackground = false;
            Owner.Slider.TickCount = 11;
            Owner.Slider.TickFrequency = 5;
            Owner.Slider.RailDarkColour = Color.FromArgb(40, Color.Black);
            Owner.Slider.TickDisplay = GH_SliderTickDisplay.Simple;
            Owner.Slider.RailDisplay = GH_SliderRailDisplay.Simple;
            Owner.Slider.Padding = new Padding(6, 2, 6, 1);
            Owner.Slider.Bounds = _sliderBounds;
        }
        private void ParameterLayout()
        {
            var maxinputTextWidth = 0;
            foreach (var param in Owner.Params.Input)
            {
                maxinputTextWidth = Math.Max(maxinputTextWidth, GH_FontServer.StringWidth(param.NickName, GH_FontServer.StandardAdjusted));
            }
            var maxoutputTextWidth = 0;
            foreach (var param in Owner.Params.Output)
            {
                maxoutputTextWidth = Math.Max(maxoutputTextWidth, GH_FontServer.StringWidth(param.NickName, GH_FontServer.StandardAdjusted));
            }
            for (int i = 0; i < Owner.Params.Input.Count; i++)
            {
                var param = Owner.Params.Input[i];
                param.Attributes.Pivot = new PointF(
                    Bounds.Left - 2 * Padding,
                    _tagBounds.Bottom + i * (ParamHeight + Padding)
                );
                param.Attributes.Bounds = new RectangleF(
                    Bounds.Left,
                    _tagBounds.Bottom + i * (ParamHeight + Padding),
                    maxinputTextWidth + 20,
                    ParamHeight
                );
            }
            for (int i = 0; i < Owner.Params.Output.Count; i++)
            {
                var param = Owner.Params.Output[i];
                param.Attributes.Pivot = new PointF(
                    Bounds.Right + 2 * Padding,
                    _tagBounds.Bottom + i * (ParamHeight + Padding)
                );
                param.Attributes.Bounds = new RectangleF(
                    Bounds.Right - maxoutputTextWidth - 20,
                    _tagBounds.Bottom + i * (ParamHeight + Padding),
                    maxoutputTextWidth + 20,
                    ParamHeight
                );
            }
        }
        private void FixParamLayout()
        {
            foreach (var param in Owner.Params)
            {
                param.Attributes = new GH_LinkedParamAttributes(param, this);
            }
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button != MouseButtons.Left)
                return base.RespondToMouseDoubleClick(sender, e);

            if (sender.Viewport.Zoom >= 0.9 && _sliderBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)))
            {
                var content = Owner.Slider.GripTextPure;
                if (Owner.IsExpression)
                {
                    var parser = new GH_ExpressionParser(false);
                    try
                    {
                        parser.CacheSymbols(Owner.Expression);
                        parser.AddVariable("x", Convert.ToDouble(Owner.Slider.Value));
                        var variant = parser.Evaluate();
                        if (variant.IsNumeric)
                            content = variant.ToString();
                    }
                    catch
                    {
                        content = Owner.Slider.GripTextPure;
                    }
                }

                Owner.Slider.TextInputHandlerDelegate = TextInputHandler;
                Owner.Slider.ShowTextInputBox(sender, true, sender.Viewport.XFormMatrix(GH_Viewport.GH_DisplayMatrix.CanvasToControl), content);
                return GH_ObjectResponse.Handled;
            }

            Owner.ShowEditor();
            return GH_ObjectResponse.Handled;
        }

        private void TextInputHandler(GH_SliderBase slider, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (GH_Convert.ToDouble(text, out var value, GH_Conversion.Secondary))
            {
                Owner.RecordUndoEvent("Slider value change");
                Owner.TrySetSliderValue(Convert.ToDecimal(value));
                Owner.ExpireSolution(true);
            }
        }

        private bool _mouseOverToStart;
        private bool _mouseOverToEnd;
        private bool _mouseOverPlay;
        private bool _mouseDownToStart;
        private bool _mouseDownToEnd;
        private bool _mouseDownPlay;

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            var pt = GH_Convert.ToPoint(e.CanvasLocation);
            if (e.Button == MouseButtons.Left && _toStartBounds.Contains(pt))
            {
                Owner.RecordUndoEvent("Animation timer to start");
                Owner.Stop();
                Owner.SetSliderValue(Owner.Slider.Minimum, true);
                sender.Invalidate();
                _mouseDownToStart = true;
                return GH_ObjectResponse.Handled;
            }

            if (e.Button == MouseButtons.Left && _toEndBounds.Contains(pt))
            {
                Owner.RecordUndoEvent("Animation timer to end");
                Owner.Stop();
                Owner.SetSliderValue(Owner.Slider.Maximum, true);
                sender.Invalidate();
                _mouseDownToEnd = true;
                return GH_ObjectResponse.Handled;
            }

            if (e.Button == MouseButtons.Left && _playBounds.Contains(pt))
            {
                Owner.TogglePlay();
                sender.Invalidate();
                _mouseDownPlay = true;
                return GH_ObjectResponse.Handled;
            }

            if (Owner.Slider.MouseDown(e.WinFormsEventArgs, e.CanvasLocation))
            {
                _dragMode = 1;
                sender.Invalidate();

                var rail = Owner.Slider.Rail;
                var railWidth = Math.Max(1, rail.Right - rail.Left);
                var canvasRailWidth = decimal.Multiply(new decimal(railWidth), Convert.ToDecimal(sender.Viewport.Zoom));
                Owner.Slider.SnapDistance = decimal.Multiply(decimal.Divide(decimal.Subtract(Owner.Slider.Maximum, Owner.Slider.Minimum), canvasRailWidth), 10m);
                return GH_ObjectResponse.Capture;
            }

            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _dragMode > 0)
            {
                if (_dragMode == 1)
                {
                    if (Owner.PlayState)
                    {
                        Owner.Stop();
                        _resumeAfterDrag = true;
                    }

                    Owner.TriggerAutoSave(Owner.InstanceGuid);
                    Owner.RecordUndoEvent("Slider value change", new SliderDragUndo(Owner));
                    _dragMode = 2;
                }

                if (_dragMode == 2)
                {
                    Owner.Slider.MouseMove(e.WinFormsEventArgs, e.CanvasLocation);
                    return GH_ObjectResponse.Handled;
                }
            }
            if (this._toStartBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)))
            {
                _mouseOverToStart = true;
                Owner.OnDisplayExpired(false);
                sender.Cursor = Cursors.Hand;
                return GH_ObjectResponse.Capture;
            }
            if (_mouseOverToStart)
            {
                _mouseOverToStart = false;
                Owner.OnDisplayExpired(false);
                Grasshopper.Instances.CursorServer.ResetCursor(sender);
                return GH_ObjectResponse.Release;
            }
            if (this._toEndBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)))
            {
                _mouseOverToEnd = true;
                Owner.OnDisplayExpired(false);
                sender.Cursor = Cursors.Hand;
                return GH_ObjectResponse.Capture;
            }
            if (_mouseOverToEnd)
            {
                _mouseOverToEnd = false;
                Owner.OnDisplayExpired(false);
                Grasshopper.Instances.CursorServer.ResetCursor(sender);
                return GH_ObjectResponse.Release;
            }
            if (this._playBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)))
            {
                _mouseOverPlay = true;
                Owner.OnDisplayExpired(false);
                sender.Cursor = Cursors.Hand;
                return GH_ObjectResponse.Capture;
            }
            if (_mouseOverPlay)
            {
                _mouseOverPlay = false;
                Owner.OnDisplayExpired(false);
                Grasshopper.Instances.CursorServer.ResetCursor(sender);
                return GH_ObjectResponse.Release;
            }

            // var overButton = _buttonBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation));
            // if (overButton != _mouseOverButton)
            // {
            //     _mouseOverButton = overButton;
            //     Owner.OnDisplayExpired(false);

            //     if (_mouseOverButton)
            //         sender.Cursor = Cursors.Hand;
            //     else
            //         Instances.CursorServer.ResetCursor(sender);

            //     return GH_ObjectResponse.Handled;
            // }

            if (e.Button == MouseButtons.None && Owner.Slider.Grip.Contains(e.CanvasLocation))
            {
                Instances.CursorServer.AttachCursor(sender, "GH_NumericSlider");
                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_dragMode > 0)
            {
                _dragMode = 0;
                Owner.Slider.MouseUp(e.WinFormsEventArgs, e.CanvasLocation);

                if (_resumeAfterDrag)
                {
                    _resumeAfterDrag = false;
                    Owner.Play();
                }

                sender.Invalidate();
                return GH_ObjectResponse.Release;
            }
            if (e.Button == MouseButtons.Left)
            {
                if (this._toEndBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)) && _mouseDownToEnd)
                {
                    _mouseDownToEnd = false;
                    _mouseOverToEnd = false;
                    Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Release;
                }
                if (this._toStartBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)) && _mouseDownToStart)
                {
                    _mouseDownToStart = false;
                    _mouseOverToStart = false;
                    Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Release;
                }
                if (this._playBounds.Contains(GH_Convert.ToPoint(e.CanvasLocation)) && _mouseDownPlay)
                {
                    _mouseDownPlay = false;
                    _mouseOverPlay = false;
                    Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Release;
                }
            }
            return base.RespondToMouseUp(sender, e);
        }

        private void ButtonRender(Graphics graphics, Rectangle ButtonBound, string DisplayText, bool mouseOver, bool mouseDown)
        {
            Brush normal_colour = AnimationTimerAttributesColourSetting.ButtonColor;
            Brush hover_colour = AnimationTimerAttributesColourSetting.HoverButtonColour;
            Brush clicked_colour = AnimationTimerAttributesColourSetting.ClickedButtonColor;

            var button = AttributeUtil.RoundedRect(ButtonBound, 2);

            Brush butCol = (mouseOver) ? hover_colour : normal_colour;
            graphics.FillPath(mouseDown ? clicked_colour : butCol, button);

            // draw button edge
            Color edgeColour = AnimationTimerAttributesColourSetting.BorderColor;
            Color edgeHover = AnimationTimerAttributesColourSetting.HoverBorderColour;
            Color edgeClick = AnimationTimerAttributesColourSetting.ClickedBorderColour;
            Color edgeCol = (mouseOver) ? edgeHover : edgeColour;
            Pen pen = new Pen(mouseDown ? edgeClick : edgeCol)
            {
                Width = (mouseDown) ? 0.8f : 0.5f
            };
            graphics.DrawPath(pen, button);
            var overlay = AttributeUtil.RoundedRect(ButtonBound, 2, true);
            graphics.FillPath(new SolidBrush(Color.FromArgb(mouseDown ? 0 : mouseOver ? 40 : 60, 255, 255, 255)), overlay);
            graphics.DrawString(DisplayText, GH_FontServer.StandardAdjusted, AnimationTimerAttributesColourSetting.AnnotationTextBright, ButtonBound, GH_TextRenderingConstants.CenterCenter);
        }
    }

    public static class AnimationTimerAttributesColourSetting
    {
        static readonly Color Primary = Color.FromArgb(39, 98, 85, 85); // rgba(98, 85, 85, 0.38)
        static readonly Color Primary_light = AttributeColourUtil.WhiteOverlay(Primary, 0.32);
        static readonly Color Primary_dark = AttributeColourUtil.Overlay(Primary, Color.Black, 0.32);
        public static Brush ButtonColor => new SolidBrush(Primary);
        public static Brush ClickedButtonColor => new SolidBrush(Primary_light);
        public static Color BorderColor => Primary_dark;
        public static Color ClickedBorderColour => Primary;
        public static Color SpacerColour => Color.DarkGray;
        public static Color SliderBorderColour => Color.Black;
        public static Brush SliderColor => new SolidBrush(Primary);
        public static Brush AnnotationTextDark => Brushes.Black;
        public static Brush AnnotationTextBright => Brushes.White;
        public static Brush HoverButtonColour => new SolidBrush(AttributeColourUtil.Overlay(Primary, Color.Black, 0.04));
        public static Color HoverBorderColour => AttributeColourUtil.WhiteOverlay(Primary, 0.86);

    }
}

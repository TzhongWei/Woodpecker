using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ButtonColourStateSetting
    {
        private int _state = 0;
        public int state
        {
            get { return _state; }
            set
            {
                if (value < PrimaryColourList.Count && value >= 0)
                    _state = value;
                else
                    _state = 0;
            }
        }
        public ButtonColourStateSetting()
        {
            PrimaryColourList = new List<Color>();
            state = 0;
        }
        public ButtonColourStateSetting(List<Color> colors)
        {
            PrimaryColourList = colors;
        }
        public List<Color> PrimaryColourList {get; set;} = new List<Color>{Color.LightBlue, Color.LightGreen, Color.LightGray};
        private Color _primary => PrimaryColourList[state];
        public Color Primary_light => AttributeColourUtil.WhiteOverlay(_primary, 0.32);
        public Color Primary_dark => AttributeColourUtil.Overlay(_primary, Color.Black, 0.32);
        public Brush ButtonColour => new SolidBrush(_primary);
        public Brush ClickedButtonColour => new SolidBrush(Primary_light);
        public Color BorderColour => Primary_dark;
        public Color ClickedBorderColour => _primary;
        public Color SpaceColour => Color.DarkGray;
        public Brush AnnotationTextDark => Brushes.Black;
        public Brush AnnotationTextBright => Brushes.White;
        public Brush HoverButtonColour => new SolidBrush(AttributeColourUtil.Overlay(_primary,Color.Black, 0.04));
        public Color HoverBorderColour => AttributeColourUtil.WhiteOverlay(_primary, 0.86);

        public static implicit  operator ButtonColourStateSetting(List<Color> colors) => new ButtonColourStateSetting(colors);
    }
    public class ButtonUIAttributesState : GH_ComponentAttributes
    {
        private readonly List<string> _buttonTexts;
        private string _buttonText => _buttonTexts[_state];
        private RectangleF _buttonBounds;
        private Action _action;
        private RectangleF _spacerBounds;
        private readonly string _spacerText;
        private bool _mouseDown;
        private bool _mouseOver;
        private int _stateSet = 0;
        private int _state
        {
            get { return _stateSet; }
            set
            {
                if (value < _buttonTexts.Count && value >= 0)
                    _stateSet = value;
                else
                    _stateSet = 0;
            }
        }
        private readonly ButtonColourStateSetting _colourStateSetting;
        private float _minWidth
        {
            get
            {
                List<string> spacers = new List<string>();
                spacers.Add(_spacerText);
                float sp = AttributeUtil.MaxTextWidth(spacers, GH_FontServer.Small);
                var buttons = new List<string>();
                buttons.AddRange(_buttonTexts);
                float bt = AttributeUtil.MaxTextWidth(buttons, GH_FontServer.Standard);
                float num = Math.Max(Math.Max(sp, bt), 90);
                return num;
            }
            set { _minWidth = value; }
        }
        public ButtonUIAttributesState(GH_Component owner, List<string> displayText, Action clickHandle, ButtonColourStateSetting colourState, string spacerText = "", int initialstate = 0) : base(owner)
        {
            _buttonTexts = displayText;
            _action = clickHandle;
            _spacerText = spacerText;
            _colourStateSetting = colourState;
            colourState.state = _state;
            _state = initialstate;
        }
        protected override void Layout()
        {
            base.Layout();
            FixLayout();

            int s = 2;
            int h0 = 0;

            //spacer and title
            if(_spacerText != "")
            {
                h0 = 10;
                _spacerBounds = new RectangleF(Bounds.X, Bounds.Bottom + s / 2, Bounds.Width, h0);
            }

            int h1 = 20; // height of button
            // create text box placeholders
            _buttonBounds = new RectangleF(Bounds.X + 2 * s, Bounds.Bottom + h0 + 2 * s, Bounds.Width - 4 * s, h1);

            //update component bounds
            Bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height + h0 + h1 + 4 * s);
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if(channel == GH_CanvasChannel.Objects)
            {
                Pen spacer = new Pen(_colourStateSetting.SpaceColour);
                var font = GH_FontServer.Standard;
                // adjust fontsize to high resolution displays
                font = new Font(font.FontFamily, font.Size / GH_GraphicsUtil.UiScale, FontStyle.Regular);

                var sml = GH_FontServer.Small;
                sml = new Font(sml.FontFamily, sml.Size / GH_GraphicsUtil.UiScale, FontStyle.Regular);

                //Draw divider line
                if (this._spacerText != "")
                {
                    graphics.DrawString(_spacerText, sml, this._colourStateSetting.AnnotationTextDark, _spacerBounds, GH_TextRenderingConstants.CenterCenter);
                    graphics.DrawLine(spacer, this._spacerBounds.X, _spacerBounds.Y + _spacerBounds.Height / 2, _spacerBounds.X + (_spacerBounds.Width - GH_FontServer.StringWidth(_spacerText, sml)) / 2 - 4, _spacerBounds.Y + _spacerBounds.Height / 2);
                    graphics.DrawLine(spacer, _spacerBounds.X + (_spacerBounds.Width - GH_FontServer.StringWidth(_spacerText, sml)) / 2 + GH_FontServer.StringWidth(_spacerText, sml) + 4, _spacerBounds.Y + _spacerBounds.Height / 2, _spacerBounds.X + _spacerBounds.Width, _spacerBounds.Y + _spacerBounds.Height / 2);
                }

                // Draw button box
                var button = AttributeUtil.RoundedRect(_buttonBounds, 2);

                Brush normal_colour = this._colourStateSetting.ButtonColour;
                Brush hover_colour = this._colourStateSetting.HoverButtonColour; //ButtonColours.HoverButtonColour;
                Brush clicked_colour = this._colourStateSetting.ClickedButtonColour; // ButtonColours.ClickedButtonColour;

                Brush butCol = (_mouseOver) ? hover_colour : normal_colour;
                graphics.FillPath(_mouseDown ? clicked_colour : butCol, button);

                // draw button edge
                Color edgeColor = this._colourStateSetting.BorderColour;
                Color edgeHover = this._colourStateSetting.HoverBorderColour;
                Color edgeClick = this._colourStateSetting.ClickedBorderColour;
                Color edgeCol = (_mouseOver) ? edgeHover : edgeColor;
                Pen pen = new Pen(_mouseDown ? edgeClick : edgeCol)
                {
                    Width = (_mouseDown) ? 0.8f : 0.5f
                };
                graphics.DrawPath(pen, button);

                System.Drawing.Drawing2D.GraphicsPath overlay = AttributeUtil.RoundedRect(_buttonBounds, 2, true);
                graphics.FillPath(new SolidBrush(Color.FromArgb(_mouseDown ? 0 : _mouseOver ? 40 : 60, 255, 255, 255)), overlay);

                // draw button text
                graphics.DrawString(_buttonText, font, this._colourStateSetting.AnnotationTextBright, _buttonBounds, GH_TextRenderingConstants.CenterCenter);
            }
        }
        protected void FixLayout()
        {
            float width = this.Bounds.Width;
            float num = Math.Max(width, _minWidth);
            float num2 = 0f;

             // first check if original component must be widened
            if (num > width)
            {
                num2 = num - width; // change in width
                // update component bounds to new width
                this.Bounds = new RectangleF(
                    this.Bounds.X - num2 / 2f,
                    this.Bounds.Y,
                    num,
                    this.Bounds.Height);
            }

            // secondly update position of input and output parameter text
            // first find the maximum text width of parameters

            foreach (IGH_Param item in base.Owner.Params.Output)
            {
                PointF pivot = item.Attributes.Pivot; // original anchor location of output
                RectangleF bounds = item.Attributes.Bounds; // text box itself
                item.Attributes.Pivot = new PointF(
                    pivot.X + num2 / 2f, // move anchor to the right
                    pivot.Y);
                item.Attributes.Bounds = new RectangleF(
                    bounds.Location.X + num2 / 2f,  // move text box to the right
                    bounds.Location.Y,
                    bounds.Width,
                    bounds.Height);
            }
            // for input params first find the widest input text box as these are right-aligned
            float inputwidth = 0f;
            foreach (IGH_Param item in base.Owner.Params.Input)
            {
                if (inputwidth < item.Attributes.Bounds.Width)
                    inputwidth = item.Attributes.Bounds.Width;
            }
            foreach (IGH_Param item2 in base.Owner.Params.Input)
            {
                PointF pivot2 = item2.Attributes.Pivot; // original anchor location of input
                RectangleF bounds2 = item2.Attributes.Bounds;
                item2.Attributes.Pivot = new PointF(
                    pivot2.X - num2 / 2f + inputwidth, // move to the left, move back by max input width
                    pivot2.Y);
                item2.Attributes.Bounds = new RectangleF(
                     bounds2.Location.X - num2 / 2f,
                     bounds2.Location.Y,
                     bounds2.Width,
                     bounds2.Height);
            }
        }
        public void UpdateSelectedIndex(int state)
        {
            this._state = state;
            this._colourStateSetting.state = state;
        }
        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_buttonBounds.Contains(e.CanvasLocation))
            {
                _mouseOver = true;
                Owner.OnDisplayExpired(false);
                sender.Cursor = System.Windows.Forms.Cursors.Hand;
                return GH_ObjectResponse.Capture;
            }

            if (_mouseOver)
            {
                _mouseOver = false;
                Owner.OnDisplayExpired(false);
                Grasshopper.Instances.CursorServer.ResetCursor(sender);
                return GH_ObjectResponse.Release;
            }

            return base.RespondToMouseMove(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                 var rec = _buttonBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    _mouseDown = true;
                    Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Capture;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var rec = _buttonBounds;
                if(rec.Contains(e.CanvasLocation))
                {
                    if(_mouseDown)
                    {
                        _mouseDown = false;
                        _mouseOver = false;
                        
                        _action();

                        Owner.OnDisplayExpired(false);
                        return GH_ObjectResponse.Release;
                    }
                }
            }
            return base.RespondToMouseUp(sender, e);
        }
    }
}
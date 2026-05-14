using System;
using System.CodeDom;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class TimelineSlot
    {
        public bool Editable { get; set; } = false;
        private double _slot = 0;
        public int Offset => 2;
        public float MinimalBoxSize {get
            {
               return 12f + GH_FontServer.StringWidth(_slot.ToString(), GH_FontServer.Standard); 
            }
        }
        private RectangleF _bounds;
        private readonly Action<double> _onTextChanged;
        public RectangleF CheckBoxbounds { get; private set; }
        public RectangleF Textbounds { get; private set; }
        public RectangleF SlotBound
        {
            get => _bounds;
            private set
            {
                _bounds = value;

                var hit = _bounds;
                hit.Inflate(Offset, Offset);
                CheckBoxbounds = hit;

                Textbounds = new RectangleF(
                  _bounds.Left, _bounds.Top - Offset,
                  _slot == 0 ? _bounds.Width + 4 * Offset : GH_FontServer.StringWidth(_slot.ToString(), GH_FontServer.Standard) - 2 * Offset,
                  _bounds.Height - 2 * Offset
                );
            }
        }
        public void UpdateValue(double SlotValue)
        {
            this._slot = SlotValue;
        }
        public double SlotValue => this._slot;
        public TimelineSlotColour TimelineSlotColour { get; set; } = new TimelineSlotColour();
        private TimelineSlot(Action<double> OnTextChanged)
        {
            this._onTextChanged = OnTextChanged;
        }
        public TimelineSlot(RectangleF SlotBound, Action<double> OnTextChanged) : this(OnTextChanged)
        {
            this.SlotBound = SlotBound;
        }
        public void EndEdit()
        {
            this._isEditMode = false;
        }
        private bool _isEditMode = false;
        public bool IsEditMode => this._isEditMode;
        private bool _mouseDown = false;
        internal void MouseLeftClickDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && Editable && CheckBoxbounds.Contains(e.CanvasLocation))
            {
                _isEditMode = true;
                _mouseDown = true;
            }
            else
            {
                _isEditMode = false;
                _mouseDown = false;
            }
        }
        internal void MouseLeftClickUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && _isEditMode && CheckBoxbounds.Contains(e.CanvasLocation))
            {
                if (_mouseDown)
                {
                    _mouseDown = false;
                }
            }
        }
        internal void MouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (CheckBoxbounds.Contains(e.CanvasLocation))
            {
                this._onTextChanged(SlotValue);
            }
            else
            {
                _mouseDown = false;
                _isEditMode = false;
            }
        }
        internal void Render(Graphics graphics)
        {

            if (_isEditMode)
            {
                var RectBox = this.SlotBound;
                using (var Path = AttributeUtil.RoundedRect(RectBox, 2f))
                {
                    graphics.FillPath(this.TimelineSlotColour.BoxColour, Path);
                    graphics.DrawPath(this.TimelineSlotColour.BoxBorder, Path);
                }

                var InputBox = new RectangleF(Textbounds.Left - 5f, Textbounds.Top, Textbounds.Width + 10f, Textbounds.Height);
                graphics.FillRectangle(this.TimelineSlotColour.EditTextColour, InputBox);
                
                InputBox = new RectangleF(Textbounds.Left - 4f - 5f, Textbounds.Top, Textbounds.Width + 10f, Textbounds.Height);
                graphics.DrawString(
                    this.SlotValue.ToString(),
                GH_FontServer.Standard,
                this.TimelineSlotColour.BoxTextColour,
                InputBox,
                new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center }
                );
            }
            else
            {
                var RectBox = this.SlotBound;
                using (var Path = AttributeUtil.RoundedRect(RectBox, 2f))
                {
                    graphics.FillPath(this.TimelineSlotColour.BoxColour, Path);
                    graphics.DrawPath(this.TimelineSlotColour.BoxBorder, Path);
                }
                var TextRect = new RectangleF(Textbounds.Left - 4f, Textbounds.Top, Textbounds.Width, Textbounds.Height);
                graphics.DrawString(
                    this.SlotValue.ToString(),
                    GH_FontServer.Standard,
                    this.TimelineSlotColour.BoxTextColour,
                    TextRect,
                    new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center }
                );
            }

        }
    }

    public class TimelineSlotColour
    {
        public TimelineSlotColour(){}
        public Color TextColour { get; set; } = Color.Black;
        public Color TextEditingColour { get; set; } = Color.White;
        public Color Border { get; set; } = Color.Black;
        public Color Primary { get; set; } = Color.FromArgb(39, 98, 85, 85);  // rgba(176, 176, 176, 0.38)
        public Color Primary_Light => WhiteOverlay(Primary, 0.32);
        public Color Primary_SelectedItem { get; set; } = Color.FromArgb(38, 102, 127, 248); // rgba(102, 127, 248, 0.38)
        public Brush EditTextColour => new SolidBrush(TextEditingColour);
        public Brush BoxColour => new SolidBrush(Primary);
        public Brush BoxTextColour => new SolidBrush(TextColour);
        public Pen BoxBorder => new Pen(Border);

        public static Color WhiteOverlay(Color original, double ratio)
        {
            Color white = Color.White;
            return Color.FromArgb(255,
                (int)(ratio * white.R + (1 - ratio) * original.R),
                (int)(ratio * white.G + (1 - ratio) * original.G),
                (int)(ratio * white.B + (1 - ratio) * original.B));
        }
        public static Color Overlay(Color original, Color overlay, double ratio)
        {
            return Color.FromArgb(255,
                (int)(ratio * overlay.R + (1 - ratio) * original.R),
                (int)(ratio * overlay.G + (1 - ratio) * original.G),
                (int)(ratio * overlay.B + (1 - ratio) * original.B));
        }
    }
}
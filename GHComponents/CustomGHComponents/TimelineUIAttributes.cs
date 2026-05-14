using Grasshopper.Kernel;
using Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using System.Drawing;
using Grasshopper.GUI;
using Grasshopper.Kernel.Special;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class TimelineUIAttributes : GH_ComponentAttributes
    {
        public TimelineUIAttributes(GH_Component owner,
        Action<double> FromOnChange,
        Action<double> ToOnChange, double FromValue, double ToValue
        ) : base(owner)
        {
            this._fromOnChange = FromOnChange;
            this._toOnChange = ToOnChange;
            this._fromValue = FromValue;
            this._toValue = ToValue;

        }
        public bool IsEditMode = false;
        private bool _fromUpdate = false;
        private bool _toUpdate = false;
        private bool _fromMouseDown = false;
        private bool _toMouseDown = false;
        private TimelineSlot _fromSlot;
        private TimelineSlot _toSlot;
        private Action<double> _fromOnChange;
        private Action<double> _toOnChange;
        private float _middleBoxWidth = 20f;
        private double _fromValue;
        private double _toValue;
        private RectangleF _timeslotFromBounds;
        private RectangleF _timeslotToBounds;
        private float _padding = 4f;
        private float _maxTextWidth = 0f;
        /// <summary>
        /// Setting the params location
        /// </summary>
        private void FixLayout()
        {
            //    --------=-------------
            // -> | In [[a] => [b]] Out| ->
            //    ----------------------
            float width = Bounds.Width;

            _maxTextWidth = 0f;
            var ListOfParams = new List<IGH_Param>(base.Owner.Params.Input);
            ListOfParams.AddRange(base.Owner.Params.Output);
            //get the Maximum textwidth
            foreach (var item in ListOfParams)
            {
                _maxTextWidth = Math.Max(_maxTextWidth, item.Attributes.Bounds.Width);
            }
            this.Bounds = new RectangleF(this.Bounds.Left - _maxTextWidth, this.Bounds.Top - _padding, this.Bounds.Width + 2 * _maxTextWidth, this.Bounds.Height + 2 * _padding);

            foreach (var item in base.Owner.Params.Output)
            {
                var pivot = new PointF(item.Attributes.Pivot.X, item.Attributes.Pivot.Y); // <= current position
                var paramsBounds = item.Attributes.Bounds; // <= Current text bounds

                item.Attributes.Pivot = new PointF(
                    Bounds.Right - _padding / 2f - paramsBounds.Width,
                    pivot.Y
                );
                item.Attributes.Bounds = new RectangleF(
                    Bounds.Right - _padding / 2f - paramsBounds.Width,
                    paramsBounds.Location.Y,
                    paramsBounds.Width,
                    paramsBounds.Height
                );
            }
            foreach (var item in base.Owner.Params.Input)
            {
                var pivot = new PointF(item.Attributes.Pivot.X, item.Attributes.Pivot.Y);
                var paramsBounds = item.Attributes.Bounds;

                item.Attributes.Pivot = new PointF(
                    Bounds.Left + _padding / 2f,
                    pivot.Y
                );
                item.Attributes.Bounds = new RectangleF(
                    Bounds.Left + _padding / 2f,
                    paramsBounds.Location.Y,
                    paramsBounds.Width,
                    paramsBounds.Height
                );
            }
        }
        private RectangleF _valueBounds;
        private float _minimalBoxSize
        {
            get
            {
                return 12f +
                Math.Max(
                GH_FontServer.StringWidth(this._fromValue.ToString(), GH_FontServer.Standard),
                GH_FontServer.StringWidth(this._toValue.ToString(), GH_FontServer.Standard)
                );
            }
        }
        protected override void Layout()
        {
            base.Layout();

            //Set up the ValueBounds
            float width = Bounds.Width;
            var SlotHeight = Bounds.Height;
            float height = SlotHeight + 2 * _padding;

            var _fromSlotSize = 12f + GH_FontServer.StringWidth(this._fromValue.ToString(), GH_FontServer.Standard);
            var _toSlotSize = 12f + GH_FontServer.StringWidth(this._toValue.ToString(), GH_FontServer.Standard);

            float num = _padding * 2 + 50f + _fromSlotSize + _toSlotSize + _middleBoxWidth;

            float num2 = num - width;
            this.Bounds = new RectangleF(this.Bounds.X - num2 / 2f, this.Bounds.Y - _padding, num, height); // this.Bounds.X - num2 / 2f => Place the text and icon at the centre


            FixLayout();
            this._valueBounds = new RectangleF(this.Bounds.Left + 1.5f * _maxTextWidth, this.Bounds.Top + _padding, this.Bounds.Width - 3f * _maxTextWidth, this.Bounds.Height - 2 * _padding);
            var fromSlotSize = Math.Max(height, _fromSlotSize) - 2 * _padding;
            var toSlotSize = Math.Max(height, _toSlotSize) - 2 * _padding;

            this._timeslotFromBounds = new RectangleF(
            this._valueBounds.Left + _padding,
            this._valueBounds.Top + _padding,
            fromSlotSize,
            SlotHeight);

            this._timeslotToBounds = new RectangleF(
                this._valueBounds.Right - _padding - toSlotSize,
                this._valueBounds.Top + _padding,
                toSlotSize,
                SlotHeight);


            this._fromSlot = new TimelineSlot(_timeslotFromBounds, this._fromOnChange);
            this._fromSlot.UpdateValue(_fromValue);
            this._toSlot = new TimelineSlot(_timeslotToBounds, this._toOnChange);
            this._toSlot.UpdateValue(_toValue);
            this._fromSlot.Editable = this.IsEditMode;
            this._toSlot.Editable = this.IsEditMode;
        }
        public void SetFromValue(double FromValue)
        {
            this._fromValue = FromValue;
        }
        public void SetToValue(double ToValue)
        {
            this._toValue = ToValue;
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            base.Render(canvas, graphics, channel);



            using (var valueBox = AttributeUtil.RoundedRect(_valueBounds, 4f))
            {
                graphics.FillPath(TimelineUIColour.ValueBoxColourBrush, valueBox);
                graphics.DrawPath(TimelineUIColour.ValueBoxBorderColourPen, valueBox);
            }
            this._fromSlot.Render(graphics);
            this._toSlot.Render(graphics);
        }
        private bool _fromMouseOver = false;
        private bool _toMouseOver = false;
        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // _fromMouseOver = _fromSlot != null && _fromSlot.CheckBoxbounds.Contains(e.CanvasLocation);
            // _toMouseOver = _toSlot != null && _toSlot.CheckBoxbounds.Contains(e.CanvasLocation);

            // bool changed = _fromMouseOver || _toMouseOver;

            // if (changed)
            // {
            //     Owner.OnDisplayExpired(false);
            //     sender.Cursor = System.Windows.Forms.Cursors.Hand;
            //     return GH_ObjectResponse.Capture;
            // }
            // if (_fromMouseDown && _fromSlot.IsEditMode || _toMouseDown && _fromSlot.IsEditMode)
            // {
            //     sender.Cursor = Cursors.IBeam;
            //     Owner.OnDisplayExpired(false);
            //     return GH_ObjectResponse.Capture;
            // }
            // if (!_fromMouseDown && _fromSlot.IsEditMode)
            // {
            //     _fromSlot.EndEdit();
            //     Owner.OnDisplayExpired(false);
            //     Grasshopper.Instances.CursorServer.ResetCursor(sender);
            //     return GH_ObjectResponse.Release;
            // }
            // if (!_toMouseDown && _toSlot.IsEditMode)
            // {
            //     _toSlot.EndEdit();
            //     Owner.OnDisplayExpired(false);
            //     Grasshopper.Instances.CursorServer.ResetCursor(sender);
            //     return GH_ObjectResponse.Release;
            // }


            // if(_fromMouseOver)
            // {
            //     _fromMouseOver = false;

            //     _fromSlot.EndEdit();
            //     Owner.OnDisplayExpired(false);
            //     Grasshopper.Instances.CursorServer.ResetCursor(sender);
            //     return GH_ObjectResponse.Release;
            // }
            // if(_toMouseOver)
            // {
            //     _toMouseOver = false;
            //     _toSlot.EndEdit();
            //     Owner.OnDisplayExpired(false);
            //     Grasshopper.Instances.CursorServer.ResetCursor(sender);
            //     return GH_ObjectResponse.Release;
            // }

            // // if (_fromMouseOver || _toMouseOver)
            // //     return GH_ObjectResponse.Handled;


            return base.RespondToMouseMove(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {

            bool hitFrom = _fromSlot != null && _fromSlot.CheckBoxbounds.Contains(e.CanvasLocation);
            bool hitTo = _toSlot != null && _toSlot.CheckBoxbounds.Contains(e.CanvasLocation);
            bool isCapture = false;
            if (e.Button == MouseButtons.Left && IsEditMode)
            {
                if (hitFrom)
                {
                    _fromMouseDown = true;
                    _fromSlot.MouseLeftClickDown(sender, e);
                    Owner.OnDisplayExpired(false);
                    isCapture = true;
                }
                else
                {
                    _fromMouseDown = false;
                    _fromMouseOver = false;
                    if(_fromSlot.IsEditMode) _fromSlot.EndEdit();
                    Owner.OnDisplayExpired(false);
                }
                if (hitTo)
                {
                    _toMouseDown = true;
                    _toSlot.MouseLeftClickDown(sender, e);
                    Owner.OnDisplayExpired(false);
                    isCapture = true;
                }
                else
                {
                    _toMouseDown = false;
                    _toMouseOver = false;
                    if(_toSlot.IsEditMode) _toSlot.EndEdit();
                    Owner.OnDisplayExpired(false);
                }
                if(isCapture)
                    return GH_ObjectResponse.Capture;
            }

            // // If slot is in the editmode and the e.location isn't on the checkingbox, -> closed edit mode
            // if (IsEditMode && !hitFrom && !hitTo)
            // {
            //     _fromSlot?.EndEdit();
            //     _toSlot?.EndEdit();

            //     Owner.OnDisplayExpired(false);
            //     return GH_ObjectResponse.Release;
            // }
            // if (e.Button == MouseButtons.Left)
            // {
            //     if (IsEditMode)
            //     {
            //         if (hitFrom && _fromMouseOver)
            //         {
            //             _fromMouseDown = true;
            //             _fromSlot.MouseLeftClickDown(sender, e);
            //             Owner.OnDisplayExpired(false);
            //             return GH_ObjectResponse.Capture;
            //         }
            //         else
            //         {
            //             _fromMouseDown = false;
            //             _fromMouseOver = false;
            //         }

            //         if (hitTo && _toMouseOver)
            //         {
            //             _toMouseDown = true;
            //             _toSlot.MouseLeftClickDown(sender, e);
            //             Owner.OnDisplayExpired(false);
            //             return GH_ObjectResponse.Capture;
            //         }
            //         else
            //         {
            //             _toMouseDown = false;
            //             _toMouseOver = false;
            //         }
            //     }
            // }


            return base.RespondToMouseDown(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (IsEditMode)
            {
                if (_fromSlot.CheckBoxbounds.Contains(e.CanvasLocation))
                {
                    this._fromSlot.MouseLeftClickUp(sender, e);
                    Owner.OnDisplayExpired(false);
                    //this._fromOnChange(this._fromValue);
                    return GH_ObjectResponse.Handled;
                }
                else
                {
                    Owner.OnDisplayExpired(false);
                }
                if (_toSlot.CheckBoxbounds.Contains(e.CanvasLocation))
                {
                    this._toSlot.MouseLeftClickUp(sender, e);
                    Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Handled;
                }
                else
                {
                    Owner.OnDisplayExpired(false);
                }
            }
            return base.RespondToMouseUp(sender, e);
        }
    }

    public static class TimelineUIColour
    {
        public static readonly Color ValueBoxColour = Color.FromArgb(18, 197, 207, 215); // rgba(197, 207, 215, 0.18)
        public static readonly Color ValueBoxBorderColour = Color.FromArgb(66, 66, 66);  //rgb(66, 66, 66)
        public static Brush ValueBoxColourBrush => new SolidBrush(ValueBoxColour);
        public static Pen ValueBoxBorderColourPen => new Pen(ValueBoxBorderColour);
    }
}
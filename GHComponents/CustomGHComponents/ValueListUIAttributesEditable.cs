using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Special;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ValueListUIAttributesEditable<T> : GH_ComponentAttributes where T : ValueListItem, new()
    {
        private RectangleF _headerBounds;
        private RectangleF _unfoldOptionBoxBounds;
        private RectangleF _foldOptionBoxBounds;
        public const int SelectionPanelHeight = 20;
        public const int Padding = 2;
        private readonly Action<int> _onSelected;
        private Action _showEditor;
        protected bool mouseOver;
        protected bool mouseDown;
        private List<bool> _mouseOverItem = new List<bool>();
        private List<bool> _mouseDownItem = new List<bool>();
        private int ItemMaximumWidth
        {
            get
            {
                int num = 20;
                foreach (var item in this._valueListItems)
                {
                    int val = GH_FontServer.StringWidth(item.Name, GH_FontServer.Standard);
                    num = Math.Max(num, val);
                }
                return checked(num + 10);
            }
        }
        private List<T> _valueListItems;
        private string _selectionTitle;
        private int _selectionIndex;
        private bool _unfolded = false;
        public ValueListUIAttributesEditable(GH_Component owner, Action<int> onSelected, Action showEditor, List<T> valueListItems, string selectionTitle, int selectedIndex = -1) : base(owner)
        {
            this._onSelected = onSelected;
            this._valueListItems = valueListItems ?? new List<T>();
            this._selectionTitle = selectionTitle ?? string.Empty;
            this._selectionIndex = selectedIndex;
            this._showEditor = showEditor;
        }
        // layout and render
        private void FixLayout()
        {
            float width = this.Bounds.Width;
            float buttonWidth = SelectionPanelHeight / 2f;

            float num = 2 * Padding + Math.Max(width, ItemMaximumWidth) + buttonWidth;
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
            ///

            _headerBounds = Bounds;

            float minimumSelectionBoxWidth = 2 * Padding + ItemMaximumWidth + buttonWidth;

            float totalWidth = Padding + Math.Max(_headerBounds.Width, minimumSelectionBoxWidth) + Padding;

            // update the position of input and output parameter text
            // first find the maximum text width of parameters

            foreach (IGH_Param item in base.Owner.Params.Output)
            {
                PointF pivot = item.Attributes.Pivot; // original anchor location of output
                var paramsbouds = item.Attributes.Bounds; //text box itself
                item.Attributes.Pivot = new PointF(
                    pivot.X + num2 / 2f + 2f + Padding,
                    pivot.Y
                );
                item.Attributes.Bounds = new RectangleF(
                    paramsbouds.Location.X + num2 / 2f + 2f + Padding,
                    paramsbouds.Location.Y,
                    paramsbouds.Width,
                    paramsbouds.Height
                );
            }

            // for input params first find the widest input text box as these are right-aligned
            var inputwidth = 0f;
            foreach (IGH_Param item in base.Owner.Params.Input)
            {
                if (inputwidth < item.Attributes.Bounds.Width)
                    inputwidth = item.Attributes.Bounds.Width;
            }
            foreach (IGH_Param item2 in base.Owner.Params.Input)
            {
                PointF pivot2 = item2.Attributes.Pivot; // original anchor location of input
                var paramsbounds = item2.Attributes.Bounds;
                item2.Attributes.Pivot = new PointF(
                    pivot2.X - num2 / 2f + inputwidth,
                    pivot2.Y
                );
                item2.Attributes.Bounds = new RectangleF(
                    paramsbounds.Location.X - num2 / 2f,
                    paramsbounds.Location.Y,
                    paramsbounds.Width,
                    paramsbounds.Height
                );
            }

            float totalHeight = _headerBounds.Height + Padding * 2 + SelectionPanelHeight + Padding;
            float selectionBoxWidth = Math.Max(
                minimumSelectionBoxWidth,
                totalWidth - 2 * Padding) - 2 * Padding;
            float selectionBoxX = _headerBounds.X + (totalWidth - selectionBoxWidth) / 2f - Padding;
            this._foldOptionBoxBounds = new RectangleF(
                selectionBoxX,
                _headerBounds.Y + _headerBounds.Height + 2f,
                selectionBoxWidth,
                SelectionPanelHeight);

            Bounds = new RectangleF(_headerBounds.X, _headerBounds.Y, totalWidth, totalHeight);
            _mouseOverItem = Enumerable.Repeat(false, _valueListItems.Count).ToList();
            _mouseDownItem = Enumerable.Repeat(false, _valueListItems.Count).ToList();
        }
        private void DynamicLayout()
        {
            var optionGaps = 2f;

            var optionPanelWidth = _foldOptionBoxBounds.Width;
            var optionPanelHeight = _foldOptionBoxBounds.Height;
            var x = _foldOptionBoxBounds.X;
            var y = _foldOptionBoxBounds.Bottom + Padding + optionGaps;
            var totalHeight = 0f;

            for (int i = 0; i < _valueListItems.Count; i++)
            {
                var itemBounds = new RectangleF(
                    x + 6f,
                    y,
                    optionPanelWidth - Padding - 6f,
                    optionPanelHeight);

                this._valueListItems[i].SetCheckListBounds(itemBounds);
                y += optionGaps + optionPanelHeight;
                totalHeight += optionGaps + optionPanelHeight;
            }

            totalHeight += 2 * optionGaps + optionPanelHeight;

            _unfoldOptionBoxBounds = new RectangleF(
                x,
                _foldOptionBoxBounds.Top,
                optionPanelWidth,
                totalHeight);
        }
        protected override void Layout()
        {
            base.Layout();
            FixLayout();
            DynamicLayout();
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            EnsureMouseState();

            if (channel == GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                float y = _foldOptionBoxBounds.Top + Padding / 2f;
                var listRectBounds = new RectangleF(
                    _foldOptionBoxBounds.Left + Padding,
                    y,
                    _foldOptionBoxBounds.Width,
                    SelectionPanelHeight
                );

                var normalColour = ValueListUIColours.ListColour;
                var hoverColour = ValueListUIColours.HoverListColour;
                var clickListColour = ValueListUIColours.ClickedOptionColour;

                var butCol = mouseOver ? hoverColour : normalColour;

                var edgeColor = ValueListUIColours.BorderColour;
                var edgeHover = ValueListUIColours.HoverBorderColour;
                var edgeClick = ValueListUIColours.ClickedBorderColour;

                var edgeCol = mouseOver ? edgeHover : edgeColor;

                using (var pen = new Pen(mouseDown ? edgeClick : edgeCol)
                {
                    Width = mouseDown ? 0.8f : 0.5f
                })
                {
                    using (var listCol = AttributeUtil.RoundedRect(listRectBounds, 4f))
                    {
                        graphics.FillPath(mouseDown ? clickListColour : butCol, listCol);
                        graphics.DrawPath(pen, listCol);
                    }
                }
                using (var overlayBrush = new SolidBrush(Color.FromArgb(mouseDown ? 0 : mouseOver ? 40 : 60, 255, 255, 255)))
                using (var overlay = AttributeUtil.RoundedRect(listRectBounds, 2, true))
                {
                    graphics.FillPath(overlayBrush, overlay);
                }

                var titleTextRect = new RectangleF(
                    _foldOptionBoxBounds.Left + 4f + Padding,
                    _foldOptionBoxBounds.Top + Padding / 2,
                    _foldOptionBoxBounds.Width,
                    SelectionPanelHeight);

                graphics.DrawString(
                    _selectionIndex < 0 || _selectionIndex > _valueListItems.Count - 1 ? _selectionTitle : _valueListItems[_selectionIndex].Name,
                    GH_FontServer.Standard,
                    _unfolded ? ValueListUIColours.TextColour : ValueListUIColours.ButtonTextColour,
                    titleTextRect,
                    new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    });

                return;
            }

            if (channel == GH_CanvasChannel.Overlay && _unfolded)
            {
                float y = _unfoldOptionBoxBounds.Top + Padding / 2f;

                var unfoldedListRectBounds = new RectangleF
                (_unfoldOptionBoxBounds.Left + Padding, y, _unfoldOptionBoxBounds.Width, _unfoldOptionBoxBounds.Height);

                var backgroundColour = ValueListUIColours.OptionListColour;
                using (var pen = new Pen(ValueListUIColours.BorderColour))
                using (var listCol = AttributeUtil.RoundedRect(unfoldedListRectBounds, 4f))
                {
                    graphics.FillPath(backgroundColour, listCol);
                    graphics.DrawPath(pen, listCol);
                }

                var selecteddItemColor = ValueListUIColours.SelectedItemColour;

                List<RectangleF> itemRectBounds = _valueListItems.Select(x =>
                {
                    var itemBounds = x.BoxName;
                    return new RectangleF(itemBounds.Left, itemBounds.Top, itemBounds.Width, itemBounds.Height);
                }).ToList();

                for (int i = 0; i < _mouseOverItem.Count; i++)
                {
                    using (var itemCol = AttributeUtil.RoundedRect(itemRectBounds[i], 4f))
                    {
                        graphics.FillPath(_mouseOverItem[i] ? selecteddItemColor : backgroundColour, itemCol);
                    }
                    var textRect = new RectangleF(
                        itemRectBounds[i].Left + 2f,
                        itemRectBounds[i].Top,
                        itemRectBounds[i].Width,
                        itemRectBounds[i].Height);

                    graphics.DrawString(
                        _valueListItems[i].Name,
                        GH_FontServer.Standard,
                        ValueListUIColours.TextColour,
                        textRect,
                        new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center
                        });
                }

                var TitletextRect = new RectangleF(_foldOptionBoxBounds.Left + 4f + Padding, _foldOptionBoxBounds.Top + Padding / 2, _foldOptionBoxBounds.Width, SelectionPanelHeight);
                graphics.DrawString(
                    _selectionIndex < 0 || _selectionIndex > _valueListItems.Count - 1 ? this._selectionTitle : this._valueListItems[_selectionIndex].Name,
                    GH_FontServer.Standard,
                    _unfolded ? ValueListUIColours.TextColour : ValueListUIColours.ButtonTextColour,
                    TitletextRect,
                    new StringFormat { LineAlignment = StringAlignment.Center }
                );
                return;
            }
            base.Render(canvas, graphics, channel);
        }
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this._showEditor();
                Layout();
                return GH_ObjectResponse.Handled;
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
        public void UpdateSelectedIndex(int index)
        {
            this._selectionIndex = index;
            EnsureMouseState();
            Layout();
        }
        public void UpdateValueList(List<T> valueLists)
        {
            this._valueListItems = valueLists;
            EnsureMouseState();
            Layout();
        }
        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            this.mouseDown = false;
            if (!_unfolded)
            {
                if (this._foldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    mouseOver = true;
                    base.Owner.OnDisplayExpired(false);
                    sender.Cursor = Cursors.Hand;
                    return GH_ObjectResponse.Capture;
                }
                if (mouseOver)
                {
                    mouseOver = false;
                    Owner.OnDisplayExpired(false);
                    _unfolded = false;
                    Grasshopper.Instances.CursorServer.ResetCursor(sender);
                    return GH_ObjectResponse.Release;
                }
            }
            else
            {
                if (this._unfoldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    var changed = false;
                    for (int i = 0; i < this._valueListItems.Count; i++)
                    {
                        var itemBounds = this._valueListItems[i].BoxName;
                        var isOver = itemBounds.Contains(e.CanvasLocation);
                        changed |= this._mouseOverItem[i] != isOver;
                        this._mouseOverItem[i] = isOver;
                        mouseOver = true;
                        sender.Cursor = Cursors.Hand;
                    }
                    if (changed)
                        Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Capture;
                }
                if (mouseOver)
                {
                    mouseOver = false;
                    Owner.OnDisplayExpired(false);
                    _unfolded = false;
                    Grasshopper.Instances.CursorServer.ResetCursor(sender);
                    return GH_ObjectResponse.Release;
                }
            }
            return base.RespondToMouseMove(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var rec = this._foldOptionBoxBounds;
                if (rec.Contains(e.CanvasLocation) && this._valueListItems.Count > 0)
                {
                    mouseDown = true;
                    Owner.OnDisplayExpired(false);
                    _unfolded = true;
                    return GH_ObjectResponse.Capture;
                }
            }
            if (_unfolded && e.Button == MouseButtons.Left)
            {

                if (this._unfoldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    for (int i = 0; i < this._valueListItems.Count; i++)
                    {
                        var itemBounds = _valueListItems[i].BoxName;
                        if (itemBounds.Contains(e.CanvasLocation))
                        {
                            this._mouseDownItem[i] = true;

                        }
                        else
                        {
                            this._mouseDownItem[i] = false;
                        }
                    }

                    Owner.OnDisplayExpired(false);
                    return GH_ObjectResponse.Capture;
                }
            }
            else
            {
                mouseDown = false;
                _unfolded = false;
                Owner.OnDisplayExpired(false);
            }

            return base.RespondToMouseDown(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var rec = this._foldOptionBoxBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    if (mouseDown)
                    {
                        mouseDown = false;
                        mouseOver = false;
                        _unfolded = true;
                        Owner.OnDisplayExpired(false);

                        return GH_ObjectResponse.Handled;
                    }
                }
            }
            if (_unfolded && e.Button == MouseButtons.Left)
            {
                if (this._unfoldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    for (int i = 0; i < this._valueListItems.Count; i++)
                    {
                        var itemBound = _valueListItems[i].BoxName;
                        if (itemBound.Contains(e.CanvasLocation))
                        {

                            mouseDown = false;
                            this._mouseDownItem[i] = false;
                            mouseOver = false;
                            this._mouseOverItem[i] = false;

                            this._onSelected(i);
                            this._selectionIndex = i;

                            this._unfolded = false;

                            Owner.OnDisplayExpired(false);

                            return GH_ObjectResponse.Handled;

                        }
                    }
                    return GH_ObjectResponse.Capture;
                }
            }
            return base.RespondToMouseUp(sender, e);
        }
        private void EnsureMouseState()
        {
            if (_mouseOverItem.Count != _valueListItems.Count)
                _mouseOverItem = Enumerable.Repeat(false, _valueListItems.Count).ToList();
            if (_mouseDownItem.Count != _valueListItems.Count)
                _mouseDownItem = Enumerable.Repeat(false, _valueListItems.Count).ToList();
        }
        
    }
}

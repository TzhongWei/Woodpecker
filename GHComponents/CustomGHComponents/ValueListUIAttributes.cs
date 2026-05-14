using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class ValueListUIAttributes : GH_ComponentAttributes
    {
        public List<string> selectionList { get; private set; }

        private readonly string _selectionTitle;
        private readonly Action<int> _onSelected;

        private RectangleF _headerBounds;
        private RectangleF _unfoldOptionBoxBounds;
        private RectangleF _foldOptionBoxBounds;
        private List<RectangleF> _itemBounds;
        protected bool mouseOver;
        protected bool mouseDown;
        protected bool mouseUp;
        private int _selectionIndex = -1;
        private bool _unfolded = false;
        private List<bool> _mouseOverItem = new List<bool>();
        private List<bool> _mouseDownItem = new List<bool>();
        public ValueListUIAttributes(GH_Component owner, Action<int> OnSelected, List<string> SelectionList, string SelectionTitle = "Selection List", int SelectedIndex = -1) : base(owner)
        {
            this.selectionList = SelectionList;

            if (selectionList.Count == 0)
                this.selectionList.Add("No Options");
            this._selectionIndex = SelectedIndex;
            this._onSelected = OnSelected;
            this._selectionTitle = SelectionTitle;
        }
        private float _selectionPanelHeight = 20f;
        private float padding = 4f;

        private float _minWidth
        {
            get
            {
                var TextList = new List<string>(this.selectionList);
                TextList.Add(this._selectionTitle);

                float MaxselectionTextWidth = 90f;
                foreach (var text in TextList)
                {
                    MaxselectionTextWidth = Math.Max(MaxselectionTextWidth, GH_FontServer.StringWidth(text, GH_FontServer.Standard));
                }
                return MaxselectionTextWidth;
            }
        }
        /// <summary>
        /// Initial Layout
        /// </summary>
        private void FixLayout()
        {
            //    --------------------
            // -> | In           Out | ->
            //    | [OptionList]     |
            //    --------------------


            float width = this.Bounds.Width; // initial component width before UI overrides
            float buttonWidth = _selectionPanelHeight / 2f;

            float num = 2 * padding + Math.Max(width, _minWidth) + buttonWidth; // number for new width
            float num2 = 0f; // value for increased width (if any)

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


            //   | padding [MaxSelectionListTextWidth | buttonWidth | ] padding |
            float SelectionPanelWidth = padding + _minWidth + buttonWidth + padding;

            // Adjust the width

            //   |         _headerBounds                             |
            //   | padding [MaxSelectionListTextWidth] padding       |
            float totalWidth = padding + Math.Max(_headerBounds.Width, SelectionPanelWidth) + padding;

            // update the position of input and output parameter text
            // first find the maximum text width of parameters

            foreach (IGH_Param item in base.Owner.Params.Output)
            {
                PointF pivot = item.Attributes.Pivot; // original anchor location of output
                var paramsbouds = item.Attributes.Bounds; //text box itself
                item.Attributes.Pivot = new PointF(
                    pivot.X + num2 / 2f + 2f + padding,
                    pivot.Y
                );
                item.Attributes.Bounds = new RectangleF(
                    paramsbouds.Location.X + num2 / 2f + 2f + padding,
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

            //   -------------------
            //.  -headBounds.Height
            //.  padding
            //.  SelectedPanelHeight
            //   padding
            //.  -------------------
            float totalHeight = _headerBounds.Height + padding + _selectionPanelHeight + padding;

            // only the button parts
            this._foldOptionBoxBounds = new RectangleF(_headerBounds.X, _headerBounds.Y + _headerBounds.Height + 2f, SelectionPanelWidth, _selectionPanelHeight);


            // Bounds is the general component size
            Bounds = new RectangleF(_headerBounds.X, _headerBounds.Y, totalWidth, totalHeight);

            _mouseOverItem = Enumerable.Repeat(false, selectionList.Count).ToList();
            _mouseDownItem = Enumerable.Repeat(false, selectionList.Count).ToList();
        }
        private void DynamicLayout()
        {
            // --------------------
            // |              Out | ->
            // | [OptionList]     |
            // --------------------
            // Click the list 
            // --------------------
            // |              Out | ->
            // | [OptionList]     |
            // --[Option 1  ]------
            //   [Option 2  ]
            //   [Option 3  ]

            // This part only create the option list bounds, namely
            // | [OptionList]     |
            // --[Option 1  ]------
            //   [Option 2  ]
            //   [Option 3  ]

            //FixLayout();
            var optionGaps = 2f;

            _itemBounds = new List<RectangleF>();
            var OptionPanelWidth = _foldOptionBoxBounds.Width;
            var _optionPanelHeight = _foldOptionBoxBounds.Height;



            var x = _foldOptionBoxBounds.X;
            var y = _foldOptionBoxBounds.Bottom + padding + optionGaps;

            float totalHeight = 0f;

            for (int i = 0; i < selectionList.Count; i++)
            {
                _itemBounds.Add(new RectangleF(x + 2f, y, OptionPanelWidth - padding - 4f, _optionPanelHeight));
                y += optionGaps + _optionPanelHeight;
                totalHeight += optionGaps + _optionPanelHeight;
            }
            totalHeight += 2 * optionGaps + _optionPanelHeight;

            _unfoldOptionBoxBounds = new RectangleF(x, _foldOptionBoxBounds.Top, OptionPanelWidth, totalHeight);
        }
        public void UndateList(List<string> SelectionList)
        {
            this.selectionList = SelectionList;
            this._selectionIndex = -1;
            Layout();
        }
        public void UpdateSelectedIndex(int index)
        {
            this._selectionIndex = index;
            Layout();
        }
        protected override void Layout()
        {
            base.Layout();
            FixLayout();
            DynamicLayout();
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            /*
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            // Render the headerBounds, render the whole bounds
            base.Render(canvas, graphics, channel);

            // Render option List
            if (_unfolded)
            {
                //Draw a list of options
                float y = _unfoldOptionBoxBounds.Top + padding / 2f;

                //Test
                // graphics.FillRectangle(backgroundColour, UnfoldedListRectBounds);
                // var GPath = new GraphicsPath();
                // GPath.AddRectangle(UnfoldedListRectBounds);
                // graphics.DrawPath(new Pen(ValueListUIColours.BorderColour), GPath);

                //Draw out frame
                var UnfoldedListRectBounds = new RectangleF(_unfoldOptionBoxBounds.Left + padding, y, _unfoldOptionBoxBounds.Width, _unfoldOptionBoxBounds.Height);
                // general background colour and drawing
                var backgroundColour = ValueListUIColours.OptionListColour;
                var pen = new Pen(ValueListUIColours.BorderColour);

                using (var ListCol = AttributeUtil.RoundedRect(UnfoldedListRectBounds, 4f))
                {
                    graphics.FillPath(backgroundColour, ListCol);
                    graphics.DrawPath(pen, ListCol);
                }

                //Item box colour
                var SelectedItemColour = ValueListUIColours.SelectedItemColour;

                List<RectangleF> itemRectBounds = _itemBounds.Select(x =>
                new RectangleF(
                    x.Left + 2f + padding, x.Top, x.Width, x.Height
                )).ToList();


                for (int i = 0; i < this._mouseOverItem.Count; i++)
                {
                    //Draw text
                    using (var itemCol = AttributeUtil.RoundedRect(itemRectBounds[i], 4f))
                    {
                        graphics.FillPath(_mouseOverItem[i] ? SelectedItemColour : backgroundColour, itemCol);
                    }
                    //Draw text
                    var textRect = new RectangleF(itemRectBounds[i].Left + 2f, itemRectBounds[i].Top, itemRectBounds[i].Width, itemRectBounds[i].Height);
                    graphics.DrawString(
                        this.selectionList[i],
                        GH_FontServer.Standard,
                        ValueListUIColours.TextColour,
                        textRect,
                        new StringFormat { LineAlignment = StringAlignment.Center }
                    );
                }
            }
            else
            {
                float y = _foldOptionBoxBounds.Top + padding / 2f;
                //Bounds

                //Colour Setting

                //fold option list bound
                var ListRectBounds = new RectangleF(_foldOptionBoxBounds.Left + padding, y, _foldOptionBoxBounds.Width, _selectionPanelHeight);

                //fold Option list colour
                var normal_Colour = ValueListUIColours.ListColour;
                var hover_Colour = ValueListUIColours.HoverListColour;
                var clickList_Colour = ValueListUIColours.ClickedOptionColour;

                var butCol = (mouseOver) ? hover_Colour : normal_Colour;

                //button edge colour
                var edgeColor = ValueListUIColours.BorderColour;
                var edgeHover = ValueListUIColours.HoverBorderColour;
                var edgeClick = ValueListUIColours.ClickedBorderColour;
                var edgeCol = (mouseOver) ? edgeHover : edgeColor;

                var pen = new Pen((mouseDown) ? edgeClick : edgeCol)
                {
                    Width = (mouseDown) ? 0.8f : 0.5f
                };

                //Draw 
                using (var ListCol = AttributeUtil.RoundedRect(ListRectBounds, 4f))
                {
                    graphics.FillPath(mouseDown ? clickList_Colour : butCol, ListCol);
                    graphics.DrawPath(pen, ListCol);
                }

                using (var overlay = AttributeUtil.RoundedRect(ListRectBounds, 2, true))
                {
                    graphics.FillPath(new SolidBrush(Color.FromArgb(mouseDown ? 0 : mouseOver ? 40 : 60, 255, 255, 255)), overlay);
                }

            }
            //Draw Title
            var TitletextRect = new RectangleF(_foldOptionBoxBounds.Left + 4f + padding, _foldOptionBoxBounds.Top + padding / 2, _foldOptionBoxBounds.Width, _selectionPanelHeight);
            graphics.DrawString(
                this._selectionIndex < 0 ? this._selectionTitle : this.selectionList[_selectionIndex],
                GH_FontServer.Standard,
                _unfolded ? ValueListUIColours.TextColour : ValueListUIColours.ButtonTextColour,
                TitletextRect,
                new StringFormat { LineAlignment = StringAlignment.Center }
            );
            */

            // 1. Component body and header
            if (channel == GH_CanvasChannel.Objects)
            {
                // Render the headerBounds, render the whole bounds
                base.Render(canvas, graphics, channel);

                float y = _foldOptionBoxBounds.Top + padding / 2f;

                var listRectBounds = new RectangleF(
                    _foldOptionBoxBounds.Left + padding,
                    y,
                    _foldOptionBoxBounds.Width,
                    _selectionPanelHeight);

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

                using (var overlay = AttributeUtil.RoundedRect(listRectBounds, 2, true))
                {
                    graphics.FillPath(
                        new SolidBrush(Color.FromArgb(mouseDown ? 0 : mouseOver ? 40 : 60, 255, 255, 255)),
                        overlay);
                }

                var titleTextRect = new RectangleF(
                    _foldOptionBoxBounds.Left + 4f + padding,
                    _foldOptionBoxBounds.Top + padding / 2,
                    _foldOptionBoxBounds.Width,
                    _selectionPanelHeight);

                graphics.DrawString(
                    _selectionIndex < 0 || _selectionIndex > selectionList.Count - 1 ? _selectionTitle : selectionList[_selectionIndex],
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

            // 2. Component body and option list when unfolded
            if (channel == GH_CanvasChannel.Overlay && _unfolded)
            {
                float y = _unfoldOptionBoxBounds.Top + padding / 2f;

                var unfoldedListRectBounds = new RectangleF(
                    _unfoldOptionBoxBounds.Left + padding,
                    y,
                    _unfoldOptionBoxBounds.Width,
                    _unfoldOptionBoxBounds.Height);

                var backgroundColour = ValueListUIColours.OptionListColour;

                using (var pen = new Pen(ValueListUIColours.BorderColour))
                using (var listCol = AttributeUtil.RoundedRect(unfoldedListRectBounds, 4f))
                {
                    graphics.FillPath(backgroundColour, listCol);
                    graphics.DrawPath(pen, listCol);
                }

                var selectedItemColour = ValueListUIColours.SelectedItemColour;

                List<RectangleF> itemRectBounds = _itemBounds.Select(x =>
                    new RectangleF(x.Left + 2f + padding, x.Top, x.Width, x.Height)).ToList();

                for (int i = 0; i < _mouseOverItem.Count; i++)
                {
                    using (var itemCol = AttributeUtil.RoundedRect(itemRectBounds[i], 4f))
                    {
                        graphics.FillPath(_mouseOverItem[i] ? selectedItemColour : backgroundColour, itemCol);
                    }

                    var textRect = new RectangleF(
                        itemRectBounds[i].Left + 2f,
                        itemRectBounds[i].Top,
                        itemRectBounds[i].Width,
                        itemRectBounds[i].Height);

                    graphics.DrawString(
                        selectionList[i],
                        GH_FontServer.Standard,
                        ValueListUIColours.TextColour,
                        textRect,
                        new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center
                        });
                }

                var TitletextRect = new RectangleF(_foldOptionBoxBounds.Left + 4f + padding, _foldOptionBoxBounds.Top + padding / 2, _foldOptionBoxBounds.Width, _selectionPanelHeight);
                graphics.DrawString(
                    _selectionIndex < 0 || _selectionIndex > selectionList.Count - 1  ? this._selectionTitle : this.selectionList[_selectionIndex],
                    GH_FontServer.Standard,
                    _unfolded ? ValueListUIColours.TextColour : ValueListUIColours.ButtonTextColour,
                    TitletextRect,
                    new StringFormat { LineAlignment = StringAlignment.Center }
                );

                return;
            }

            // return normal channel
            base.Render(canvas, graphics, channel);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            this.mouseDown = false;
            if (!_unfolded)
            {
                if (this._foldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    mouseOver = true;
                    Owner.OnDisplayExpired(false);
                    sender.Cursor = System.Windows.Forms.Cursors.Hand;
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
                // this._itemBounds = new List<RectangleF>(...Items)
                if (this._unfoldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    //bool hitAnyItem = false;
                    for (int i = 0; i < this._itemBounds.Count; i++)
                    {
                        if (_itemBounds[i].Contains(e.CanvasLocation))
                        {
                            this._mouseOverItem[i] = true;
                            //hitAnyItem = true;
                        }
                        else
                        {
                            this._mouseOverItem[i] = false;
                        }
                        Owner.OnDisplayExpired(false);
                        mouseOver = true;
                        sender.Cursor = System.Windows.Forms.Cursors.Hand;
                    }
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var rec = this._foldOptionBoxBounds;
                if (rec.Contains(e.CanvasLocation))
                {
                    mouseDown = true;
                    Owner.OnDisplayExpired(false);
                    _unfolded = true;
                    return GH_ObjectResponse.Capture;
                }
            }
            else
            {
                mouseDown = false;
                Owner.OnDisplayExpired(false);
                _unfolded = true;
            }
            if (_unfolded && e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                if (this._unfoldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    for (int i = 0; i < this._itemBounds.Count; i++)
                    {
                        if (_itemBounds[i].Contains(e.CanvasLocation))
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
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
            if (_unfolded && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this._unfoldOptionBoxBounds.Contains(e.CanvasLocation))
                {
                    for (int i = 0; i < this._itemBounds.Count; i++)
                    {
                        if (_itemBounds[i].Contains(e.CanvasLocation))
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
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }

    public sealed class ValueListUIColours
    {
        public static readonly Color Primary = Color.FromArgb(39, 98, 85, 85);  // rgba(176, 176, 176, 0.38)
        public static readonly Color Primary_Light = AttributeColourUtil.WhiteOverlay(Primary, 0.32);
        public static readonly Color Primary_SelectedItem = Color.FromArgb(38, 102, 127, 248); // rgba(102, 127, 248, 0.38)
        public static readonly Color Primary_Dark = AttributeColourUtil.Overlay(Primary, Color.Black, 0.32);
        public static Brush ListColour => new SolidBrush(Primary);
        public static Brush ClickedOptionColour => new SolidBrush(Primary_Light);
        public static Brush OptionListColour => new SolidBrush(Color.White);
        public static Brush SelectedItemColour => new SolidBrush(Primary_SelectedItem);
        public static Color BorderColour => Primary_Dark;
        public static Color ClickedBorderColour => Primary;
        public static Brush ButtonTextColour = new SolidBrush(Color.White);
        public static Brush TextColour => new SolidBrush(Color.Black);
        public static Brush HoverListColour => new SolidBrush(AttributeColourUtil.Overlay(Primary, Color.Black, 0.04));
        public static Color HoverBorderColour => AttributeColourUtil.WhiteOverlay(Primary, 0.86);

        

    }
}
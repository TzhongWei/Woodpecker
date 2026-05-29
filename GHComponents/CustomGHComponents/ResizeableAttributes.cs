using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.GUI;
using Woodpecker.Animation.GHComponents;


namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public abstract class ResizableAttributes<T> : GH_ComponentAttributes where T : IGH_DocumentObject, IGH_Component
    {
        private GH_ResizeBorder m_resize_data;
        public new T Owner => (T) base.Owner;
        private bool m_resize_undo;

        protected abstract Size MinimumSize { get; }

        protected virtual Size MaximumSize => new Size(int.MaxValue, int.MaxValue);

        protected abstract Padding SizingBorders { get; }

        protected ResizableAttributes(T owner)
            : base(owner)
        {
            m_resize_undo = false;
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                List<GH_Border> list = GH_Border.CreateBorders(Bounds, SizingBorders);
                foreach (GH_Border item in list)
                {
                    if (item.Contains(e.CanvasLocation))
                    {
                        m_resize_data = new GH_ResizeBorder(item);
                        m_resize_data.Setup(this, e.CanvasLocation, MinimumSize, MaximumSize);
                        return GH_ObjectResponse.Capture;
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.None)
            {
                List<GH_Border> list = GH_Border.CreateBorders(Bounds, SizingBorders);
                foreach (GH_Border item in list)
                {
                    if (item.Contains(e.CanvasLocation))
                    {
                        sender.Cursor = item.Size_Cursor;
                        if (item.Topology == GH_BorderTopology.None)
                        {
                            return base.RespondToMouseMove(sender, e);
                        }
                        return GH_ObjectResponse.Handled;
                    }
                }
            }
            if (e.Button == MouseButtons.Left && m_resize_data != null)
            {
                if (!m_resize_undo)
                {
                    m_resize_undo = true;
                    base.Owner.OnPingDocument()?.UndoUtil.RecordLayoutEvent("Resize " + base.Owner.Name, base.Owner);
                }
                m_resize_data.Solve(e.CanvasLocation, out var new_shape, out var new_pivot);
                Pivot = new_pivot;
                Bounds = new_shape;
                ExpireLayout();
                sender.Refresh();
                return GH_ObjectResponse.Handled;
            }
            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            m_resize_undo = false;
            if (m_resize_data != null)
            {
                m_resize_data = null;
                return GH_ObjectResponse.Release;
            }
            return base.RespondToMouseUp(sender, e);
        }
    }
}
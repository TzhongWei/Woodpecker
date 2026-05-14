using System.Drawing;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper;
using System.Drawing.Drawing2D;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public static class AttributeUtil
    {
        public static float MaxTextWidth(List<string> spacerTxts, Font font)
        {
            float sp = new float(); //width of spacer text

            // adjust fontsize to high resolution displays
            font = new Font(font.FontFamily, font.Size / GH_GraphicsUtil.UiScale, FontStyle.Regular);

            for (int i = 0; i < spacerTxts.Count; i++)
            {
                if (GH_FontServer.StringWidth(spacerTxts[i], font) + 8 > sp)
                    sp = GH_FontServer.StringWidth(spacerTxts[i], font) + 8;
            }
            return sp;
        }
        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            float d = radius * 2f;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
        public static GraphicsPath RoundedRect(RectangleF bounds, int radius, bool overlay = false)
        {
            RectangleF b = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            RectangleF arc = new RectangleF(b.Location, size);
            GraphicsPath path = new GraphicsPath();
            
            if (overlay)
                b.Height = diameter;

            if (radius == 0)
            {
                path.AddRectangle(b);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = b.Right - diameter;
            path.AddArc(arc, 270, 90);

            if (!overlay)
            {
                // bottom right arc  
                arc.Y = b.Bottom - diameter;
                path.AddArc(arc, 0, 90);

                // bottom left arc 
                arc.X = b.Left;
                path.AddArc(arc, 90, 90);
            }
            else
            {
                path.AddLine(new PointF(b.X + b.Width, b.Y + b.Height), new PointF(b.X, b.Y + b.Height));
            }

            path.CloseFigure();
            return path;
        }
    }
}
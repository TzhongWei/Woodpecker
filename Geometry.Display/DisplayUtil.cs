using System.Collections.Generic;
using Rhino.Geometry;
using Rhino;
using Grasshopper;
using System.Drawing;
using System;
using System.Linq;
using System.Runtime.Versioning;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.Geometry.Display
{
    public static class DisplayDefaultColour
    {
        public static Color SelectedColour = Color.LightSkyBlue;
        public static Color UnSelectedColour = Color.LightGray;
    }
    public static class DisplayUtil
    {
        /// <summary>
        /// Filter out invisible geometries based on their indices.
        /// </summary>
        /// <param name="GeoObjs">List of geometry objects.</param>
        /// <param name="Invisible">List of indices for invisible geometries.</param>
        /// <returns>List of visible geometry objects.</returns>
        public static List<GeometryBase> VisibleGeometries(List<GeometryBase> GeoObjs, List<int> Invisible)
        {
            var visible = new List<GeometryBase>();
            for (int i = 0; i < GeoObjs.Count; i++)
            {
                if (!Invisible.Contains(i))
                {
                    visible.Add(GeoObjs[i]);
                }
            }
            return visible;
        }
        /// <summary>
        /// Interpret the colours of geometries based on pointer_t. If the index of a geometry is in the Selected list, it will be colored with the SelectedColour. Otherwise, it will be colored with the DefaultColour. This method is useful for visualizing selected geometries in a different color from unselected ones.
        /// </summary>
        /// <param name="a">The first color</param>
        /// <param name="b">The second color</param>
        /// <param name="t">The interpolation factor (0 to 1)</param>
        /// <returns>The interpolated color</returns>
        public static System.Drawing.Color LerpColor(Color a, Color b, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));

            int A = (int)Math.Round(a.A + (b.A - a.A) * t);
            int R = (int)Math.Round(a.R + (b.R - a.R) * t);
            int G = (int)Math.Round(a.G + (b.G - a.G) * t);
            int B = (int)Math.Round(a.B + (b.B - a.B) * t);

            // Clamp to byte range
            A = Math.Max(0, Math.Min(255, A));
            R = Math.Max(0, Math.Min(255, R));
            G = Math.Max(0, Math.Min(255, G));
            B = Math.Max(0, Math.Min(255, B));

            return System.Drawing.Color.FromArgb(A, R, G, B);
        }
        public static List<Curve> DisplaySilhouette(GeometryBase G)
        {
            var ActV = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView;
            var AccDic = ActV.ActiveViewport.CameraDirection;
            var result = Silhouette.Compute(G, SilhouetteType.Boundary, AccDic, 0.01, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians).Select(x => x.Curve).ToList();
            if(result == null)
                throw new Exception("Silhouette failed");
            return result;
        }
        public static int GetWorldWidthFromScreenWidth(int lineWidth, Point3d referencePt)
        {
            var avp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            if(!avp.GetWorldToScreenScale(referencePt, out var pixelsPerUnit))
            {
                return lineWidth;
            }
            if(pixelsPerUnit <= 0 || double.IsNaN(pixelsPerUnit) || double.IsInfinity(pixelsPerUnit))
                return lineWidth;
            return Math.Max(1, (int)Math.Round(lineWidth / pixelsPerUnit));
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Display;
using System.Drawing;
using System;
using Rhino;

namespace Woodpecker.Animation.Control.Camera
{
    public static class CameraUtil
    {
        public static int Lerp(int a, int b, double t)
        => a + (int)Math.Round((b - a) * t);

        public static Point3d Lerp(Point3d a, Point3d b, double t)
        => new Point3d(a.X + (b.X - a.X) * t,
                       a.Y + (b.Y - a.Y) * t,
                       a.Z + (b.Z - a.Z) * t);
        public static Vector3d Lerp(Vector3d a, Vector3d b, double t)
        => new Vector3d(a.X + (b.X - a.X) * t,
                       a.Y + (b.Y - a.Y) * t,
                       a.Z + (b.Z - a.Z) * t);

        public static double Lerp(double a, double b, double t)
        => a + (b - a) * t;
        public static List<Point3d> GetTargetRectCorners(RhinoViewport viewport)
        {
            Point3d[] nearCorners = viewport.GetNearRect();
            Point3d[] farCorners = viewport.GetFarRect();
            Point3d targetPoint = viewport.CameraTarget;

            // Define the target plane at the camera target location
            Plane nearPlane;
            viewport.GetFrustumNearPlane(out nearPlane);
            Plane targetPlane = new Plane(targetPoint, nearPlane.ZAxis);

            List<Point3d> intersectionPoints = new List<Point3d>();

            // Calculate intersections between frustum edges and the target plane
            for (int i = 0; i < nearCorners.Length; i++)
            {
                Line frustumEdge = new Line(nearCorners[i], farCorners[i]);
                if (Rhino.Geometry.Intersect.Intersection.LinePlane(frustumEdge, targetPlane, out double t))
                {
                    Point3d intersectionPoint = frustumEdge.PointAt(t);
                    intersectionPoints.Add(intersectionPoint);
                }
            }
            return intersectionPoints;
        }
        public static List<Point3d> GetTargetRectCorners(ViewportInfo viewport)
        {
            Point3d[] nearCorners = viewport.GetNearPlaneCorners();
            Point3d[] farCorners = viewport.GetFarPlaneCorners();
            Point3d targetPoint = viewport.TargetPoint;

            // Define the target plane at the camera target location

            var nearPlane = viewport.FrustumNearPlane;

            //Plane targetPlane = new Plane(targetPoint, nearPlane.ZAxis);
            Plane targetPlane = new Plane(targetPoint, viewport.CameraDirection);

            List<Point3d> intersectionPoints = new List<Point3d>();

            // Calculate intersections between frustum edges and the target plane
            for (int i = 0; i < nearCorners.Length; i++)
            {
                Line frustumEdge = new Line(nearCorners[i], farCorners[i]);
                if (Rhino.Geometry.Intersect.Intersection.LinePlane(frustumEdge, targetPlane, out double t))
                {
                    Point3d intersectionPoint = frustumEdge.PointAt(t);
                    intersectionPoints.Add(intersectionPoint);
                }
            }
            return intersectionPoints;
        }
        public static List<Point3d> GetTargetRectCorners(CameraParameter camera)
        {
            if (camera == null)
                return new List<Point3d>();

            if (!camera.IsParallel)
                return GetTargetRectCorners(camera.viewportInfo);

            return GetParallelTargetRectCorners(camera);
        }
        public static List<LineCurve> DisplayCamera(Rhino.DocObjects.ViewportInfo viewport)
        {
            var nearCorners = viewport.GetNearPlaneCorners();
            //return nearCorners.ToList();
            var farCorners = viewport.GetFarPlaneCorners();
            var crvs = new List<LineCurve>();
            var index = new int[] { 0, 1, 3, 2, 0 };
            for (int i = 0; i < index.Length - 1; i++)
            {
                crvs.Add(new LineCurve(nearCorners[index[i]], nearCorners[index[i + 1]]));
                crvs.Add(new LineCurve(farCorners[index[i]], farCorners[index[i + 1]]));
            }
            for (int i = 0; i < nearCorners.Length; i++)
            {
                crvs.Add(new LineCurve(nearCorners[i], farCorners[i]));
            }
            return crvs;
        }
        public static List<LineCurve> DisplayCamera(CameraParameter camera)
        {
            if (camera == null)
                return new List<LineCurve>();

            if (!camera.IsParallel)
                return DisplayCamera(camera.viewportInfo);

            var targetCorners = GetParallelTargetRectCorners(camera);
            if (targetCorners.Count != 4)
                return DisplayCamera(camera.viewportInfo);

            var camDir = camera.CameraDirection;
            if (!camDir.Unitize())
                return DisplayCamera(camera.viewportInfo);

            var targetDistance = camera.CameraLocation.DistanceTo(camera.CameraTarget);
            var nearOffset = camera.parallelParameters.Near - targetDistance;
            var farOffset = camera.parallelParameters.Far - targetDistance;

            var fakeNearCorners = targetCorners.Select(x => x + camDir * nearOffset).ToArray();
            var fakeFarCorners = targetCorners.Select(x => x + camDir * farOffset).ToArray();

            var crvs = new List<LineCurve>();
            var index = new int[] { 0, 1, 3, 2, 0 };
            for (int i = 0; i < index.Length - 1; i++)
            {
                crvs.Add(new LineCurve(fakeNearCorners[index[i]], fakeNearCorners[index[i + 1]]));
                crvs.Add(new LineCurve(fakeFarCorners[index[i]], fakeFarCorners[index[i + 1]]));
            }
            for (int i = 0; i < fakeNearCorners.Length; i++)
            {
                crvs.Add(new LineCurve(fakeNearCorners[i], fakeFarCorners[i]));
            }
            return crvs;
        }
        public static List<LineCurve> DisplayCamera(Rhino.Display.RhinoViewport viewport)
        {
            var nearCorners = viewport.GetNearRect();
            //return nearCorners.ToList();
            var farCorners = viewport.GetFarRect();
            var crvs = new List<LineCurve>();
            var index = new int[] { 0, 1, 3, 2, 0 };
            for (int i = 0; i < index.Length - 1; i++)
            {
                crvs.Add(new LineCurve(nearCorners[index[i]], nearCorners[index[i + 1]]));
                crvs.Add(new LineCurve(farCorners[index[i]], farCorners[index[i + 1]]));
            }
            for (int i = 0; i < nearCorners.Length; i++)
            {
                crvs.Add(new LineCurve(nearCorners[i], farCorners[i]));
            }
            return crvs;
        }
        private static List<Point3d> GetParallelTargetRectCorners(CameraParameter camera)
        {
            var xAxis = camera.viewportInfo.CameraX;
            var yAxis = camera.viewportInfo.CameraY;
            if (!xAxis.Unitize() || !yAxis.Unitize())
                return GetTargetRectCorners(camera.viewportInfo);

            var values = camera.parallelParameters;
            var halfHeight = values.ParallelHeight * 0.5;
            var halfWidth = halfHeight * values.AspectRatio;
            var center = camera.CameraTarget
                + xAxis * values.OffsetX
                + yAxis * values.OffsetY;

            Point3d ToWorld(double x, double y)
                => center + xAxis * x + yAxis * y;

            return new List<Point3d>
            {
                ToWorld(-halfWidth, halfHeight),
                ToWorld(halfWidth, halfHeight),
                ToWorld(-halfWidth, -halfHeight),
                ToWorld(halfWidth, -halfHeight)
            };
        }
        public static List<Line> DisplayCameraDirection(Rhino.DocObjects.ViewportInfo viewport)
        {
            var crvs = new List<Line>();
            var X = viewport.CameraX;
            var Y = viewport.CameraY;
            var Z = -viewport.CameraZ;
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) throw new Exception("RhinoDoc.ActiveDoc cannot be null");
            doc.Views.ActiveView.ActiveViewport.GetWorldToScreenScale(viewport.CameraLocation, out var pixelsPerUnit);
            if (pixelsPerUnit <= 0 || double.IsNaN(pixelsPerUnit) || double.IsInfinity(pixelsPerUnit))
                pixelsPerUnit = 1.0;

            // GetWorldToScreenScale returns pixels per model unit.
            // To keep the preview axis a constant size on screen, convert the target pixel size back to model units.
            const double targetPixelLength = 40.0;
            var scale = targetPixelLength / pixelsPerUnit;

            X.Unitize();
            Y.Unitize();
            Z.Unitize();

            crvs.Add(new Line(viewport.CameraLocation, viewport.CameraLocation + Z * scale));
            crvs.Add(new Line(viewport.CameraLocation, viewport.CameraLocation + Y * scale));
            crvs.Add(new Line(viewport.CameraLocation, viewport.CameraLocation + X * scale));
            return crvs;
        }
        public static bool GetWorldToScreenScale(ViewportInfo viewportInfo, out double PixelsPerUnit)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                PixelsPerUnit = 0;
                return false;
            }
            var avp = doc.Views.ActiveView.ActiveViewport;
            avp.GetWorldToScreenScale(viewportInfo.TargetPoint, out PixelsPerUnit);
            return true;
        }
        public static Rectangle ViewRect(Rhino.Display.RhinoViewport viewport)
        {
            var pts = GetTargetRectCorners(viewport);
            var pts2D = pts.Select(x => viewport.WorldToClient(x)).ToList();
            var RectMaxX = (int)pts2D.Max(Pt => Pt.X);
            var RectMinX = (int)pts2D.Min(Pt => Pt.X);
            var RectMaxY = (int)pts2D.Max(Pt => Pt.Y);
            var RectMinY = (int)pts2D.Min(Pt => Pt.Y);
            int w = Math.Max(1, RectMaxX - RectMinX);
            int h = Math.Max(1, RectMaxY - RectMinY);
            return new Rectangle(RectMinX, RectMinY, w, h);
        }
        public static Rectangle ViewRect(Rhino.DocObjects.ViewportInfo viewport)
        {
            var pts = GetTargetRectCorners(viewport);
            var pts2D = pts.Select(x => RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.WorldToClient(x)).ToList();
            var RectMaxX = (int)pts2D.Max(Pt => Pt.X);
            var RectMinX = (int)pts2D.Min(Pt => Pt.X);
            var RectMaxY = (int)pts2D.Max(Pt => Pt.Y);
            var RectMinY = (int)pts2D.Min(Pt => Pt.Y);
            int w = Math.Max(1, RectMaxX - RectMinX);
            int h = Math.Max(1, RectMaxY - RectMinY);
            return new Rectangle(RectMinX, RectMinY, w, h);
        }
        [Obsolete]
        public static Rectangle InterpolateParallel(CameraParameter CurrentInfo,
            ViewportInfo viewport1,
            ViewportInfo viewport2,
            double t)
        {
            var doc = RhinoDoc.ActiveDoc;
            var vp = doc.Views.ActiveView;
            var avp = vp.ActiveViewport;

            var tempViewIndex = doc.NamedViews.Add("__temp_view__", vp.ActiveViewportID);  //Save Current View

            /// fakeVP
            avp.CameraUp = CurrentInfo.CameraUp;
            avp.SetCameraLocations(CurrentInfo.CameraTarget, CurrentInfo.CameraLocation);
            avp.ZoomWindow(CurrentInfo.WindowRect);

            var VpAcorners = CameraUtil.GetTargetRectCorners(viewport1);
            var VpBcorners = CameraUtil.GetTargetRectCorners(viewport2);
            var vpAPts2D = VpAcorners.Select(p => avp.WorldToClient(p)).ToList();
            var vpBPts2D = VpBcorners.Select(p => avp.WorldToClient(p)).ToList();
            int RectMinX = CameraUtil.Lerp((int)vpAPts2D.Min(pt => pt.X), (int)vpBPts2D.Min(pt => pt.X), t);
            int RectMaxX = CameraUtil.Lerp((int)vpAPts2D.Max(pt => pt.X), (int)vpBPts2D.Max(pt => pt.X), t);
            int RectMinY = CameraUtil.Lerp((int)vpAPts2D.Min(pt => pt.Y), (int)vpBPts2D.Min(pt => pt.Y), t);
            int RectMaxY = CameraUtil.Lerp((int)vpAPts2D.Max(pt => pt.Y), (int)vpBPts2D.Max(pt => pt.Y), t);
            int w = Math.Max(1, RectMaxX - RectMinX);
            int h = Math.Max(1, RectMaxY - RectMinY);

            var rect = new Rectangle(RectMinX, RectMinY, w, h);


            if (tempViewIndex >= 0)
            {
                /// restore the intial place
                doc.NamedViews.Restore(tempViewIndex, avp);
                doc.NamedViews.Delete(tempViewIndex);
            }
            else
            {
                throw new Exception("Error interpretation");
            }

            return rect;
        }
        public static Rectangle InterpolateRectangle(Rectangle rectA, Rectangle rectB, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));

            var left = Lerp(rectA.Left, rectB.Left, t);
            var right = Lerp(rectA.Right, rectB.Right, t);
            var top = Lerp(rectA.Top, rectB.Top, t);
            var bottom = Lerp(rectA.Bottom, rectB.Bottom, t);

            if (right <= left)
                right = left + 1;
            if (bottom <= top)
                bottom = top + 1;

            return Rectangle.FromLTRB(left, top, right, bottom);
        }
        public static double GetParallelCameraZoomFactor(CameraParameter cameraParam1, CameraParameter cameraParam2)
        {
            if(!cameraParam1.IsParallel || !cameraParam2.IsParallel)
                throw new ArgumentException("Both camera parameters must be parallel.");
            var heightA = cameraParam1.parallelParameters.ParallelHeight;
            var heightB = cameraParam2.parallelParameters.ParallelHeight;
            if (heightA <= 1e-9 || heightB <= 1e-9)
                return 1.0;
            return heightA / heightB;
        }
    }
}

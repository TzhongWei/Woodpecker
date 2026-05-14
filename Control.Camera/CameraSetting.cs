using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;


namespace Woodpecker.Animation.Control.Camera
{
    [Obsolete]
    public class CameraSetting
    {
        public CameraSetting()
        { }
        private static bool DataChecking(RhinoDoc Doc, double t, out string Message)
        {
            Message = "";
            bool DataCheckB = true;
            if(t < 0 || t > 1)
            {
                Message += "t must be in between 0 and 1 \n";
                DataCheckB |= false;
            }
            if(Doc == null)
            {
                Message += "RhinoDoc is null \n";
                DataCheckB |= false;
            }
            if(Doc.Views.ActiveView == null)
            {
                Message = "RhinoDoc ActiveView is null \n";
                DataCheckB |= false;
            }
            return DataCheckB;
        }
        // Have Bugs
        public static void CameraTraverseViews(RhinoDoc Doc, double t, List<string> ViewNames, out string Message, out Curve CameraPath)
        {
            if (!DataChecking(Doc, t, out Message))
            {
                CameraPath = null;
                return;
            }

            var rv = Doc.Views.ActiveView;

            var avp = rv.ActiveViewport;

            // ---- get named views ----
            var viewInfos = ViewNames
                .Select(n => Doc.NamedViews[Doc.NamedViews.FindByName(n)])
                .ToList();

            var vps = viewInfos.Select(v => v.Viewport).ToList();

            // ---- endpoint snap ----
            if (t <= 1e-6)
            {
                avp.PushViewInfo(viewInfos.First(), true);
                rv.Redraw();
                CameraPath = null;
                return;
            }

            if (t >= 1 - 1e-6)
            {
                avp.PushViewInfo(viewInfos.Last(), true);
                rv.Redraw();
                CameraPath = null;
                return;
            }

            // ---- projection mode ----
            bool isParallel = vps.All(v => v.IsParallelProjection);

            // ---- collect camera data ----
            var camPts = vps.Select(v => v.CameraLocation).ToList();
            var tarPts = vps.Select(v => v.TargetPoint).ToList();
            var upVecs = vps.Select(v => v.CameraUp).ToList();
            var lens = vps.Select(v => v.Camera35mmLensLength).ToList();

            // ---- curves for paths ----
            Curve camCrv = camPts.Count > 2 ? Curve.CreateInterpolatedCurve(camPts, 3) : null;
            Curve tarCrv = tarPts.Count > 2 ? Curve.CreateInterpolatedCurve(tarPts, 3) : null;
            Curve upCrv = upVecs.Count > 2
                ? Curve.CreateInterpolatedCurve(upVecs.Select(u => new Point3d(u)).ToList(), 3)
                : null;

            Point3d camCur, tarCur;
            Vector3d upCur;
            double lensCur;

            if (camCrv != null)
            {
                camCur = camCrv.PointAtNormalizedLength(t);
                tarCur = tarCrv.PointAtNormalizedLength(t);
                upCur = new Vector3d(upCrv.PointAtNormalizedLength(t));

                CameraPath = camCrv;
            }
            else
            {
                camCur = CameraUtil.Lerp(camPts[0], camPts[1], t);
                tarCur = CameraUtil.Lerp(tarPts[0], tarPts[1], t);
                upCur = CameraUtil.Lerp(upVecs[0], upVecs[1], t);

                CameraPath = new LineCurve(camPts[0], camPts[1]);
            }

            int i0 = (int)Math.Floor(t * (lens.Count - 1));
            int i1 = Math.Min(i0 + 1, lens.Count - 1);
            double localT = t * (lens.Count - 1) - i0;
            lensCur = lens[i0] + (lens[i1] - lens[i0]) * localT;

            // ---- apply ----
            if (!isParallel)
            {
                avp.SetCameraLocations(tarCur, camCur);
                avp.Camera35mmLensLength = lensCur;
                avp.CameraUp = upCur;
            }
            else
            {
                avp.ChangeToParallelProjection(true);

                avp.SetCameraLocations(tarCur, camCur);
                avp.CameraUp = upCur;

                // ---- zoom window interpolation ----
                var cornersA = CameraUtil.GetTargetRectCorners(vps[i0]);
                var cornersB = CameraUtil.GetTargetRectCorners(vps[i1]);

                var a2 = cornersA.Select(p => avp.WorldToClient(p)).ToList();
                var b2 = cornersB.Select(p => avp.WorldToClient(p)).ToList();

                int minX = CameraUtil.Lerp((int)a2.Min(p => p.X), (int)b2.Min(p => p.X), localT);
                int maxX = CameraUtil.Lerp((int)a2.Max(p => p.X), (int)b2.Max(p => p.X), localT);
                int minY = CameraUtil.Lerp((int)a2.Min(p => p.Y), (int)b2.Min(p => p.Y), localT);
                int maxY = CameraUtil.Lerp((int)a2.Max(p => p.Y), (int)b2.Max(p => p.Y), localT);

                var rect = new Rectangle(
                    minX,
                    minY,
                    Math.Max(1, maxX - minX),
                    Math.Max(1, maxY - minY)
                );

                avp.ZoomWindow(rect);
            }

            rv.Redraw();
        }
        public static void CameraFromTo(RhinoDoc Doc, double t, string FromViewName, string ToViewName, out string Message, out Curve CameraPath)
        {
            if (!DataChecking(Doc, t, out Message))
            {   
                CameraPath = null;
                return;
            }

            int idxA = Doc.NamedViews.FindByName(FromViewName);
            int idxB = Doc.NamedViews.FindByName(ToViewName);
            if (idxA < 0 || idxB < 0)
            {
                Message = "Named view not found. Check FromName / ToName.";
                CameraPath = null;
                return;
            }
            var viA = Doc.NamedViews[idxA];
            var viB = Doc.NamedViews[idxB];
            var vpA = viA.Viewport;
            var vpB = viB.Viewport;

            var DirA = vpA.CameraDirection;
            var DirB = vpB.CameraDirection;
            var CamA = vpA.CameraLocation;
            var CamB = vpB.CameraLocation;
            var tarA = vpA.TargetPoint;
            var tarB = vpB.TargetPoint;
            var lensA = vpA.Camera35mmLensLength;
            var lensB = vpB.Camera35mmLensLength;
            var CamupA = vpA.CameraUp;
            var CamupB = vpB.CameraUp;


            var CamCur = CameraUtil.Lerp(CamA, CamB, t);
            var tarCur = CameraUtil.Lerp(tarA, tarB, t);
            var DirCur = CameraUtil.Lerp(DirA, DirB, t);
            var lensCur = lensA + (lensB - lensA) * t;
            var CamupCur = CameraUtil.Lerp(CamupA, CamupB, t);

            var rv = Doc.Views.ActiveView;

            var avp = rv.ActiveViewport;

            var IsParallel = vpA.IsParallelProjection && vpB.IsParallelProjection;
            if (t <= 1e-6)
            {
                avp.PushViewInfo(viA, true);
                goto REDRAW;
            }
            else if (t >= 1 - 1e-6)
            {
                avp.PushViewInfo(viB, true);
                goto REDRAW;
            }


            if (!IsParallel)
            {
                avp.SetCameraLocations(tarCur, CamCur);
                avp.Camera35mmLensLength = lensCur;
                avp.CameraUp = CamupCur;
            }
            else
            {
                avp.SetCameraLocations(tarCur, CamCur);
                avp.CameraUp = CamupCur;
                //Control focus with ZoomWindows
                var VpAcorners = CameraUtil.GetTargetRectCorners(vpA);
                var VpBcorners = CameraUtil.GetTargetRectCorners(vpB);

                var vpAPts2D = VpAcorners.Select(p => avp.WorldToClient(p)).ToList();
                var vpBPts2D = VpBcorners.Select(p => avp.WorldToClient(p)).ToList();
                int RectMinX = CameraUtil.Lerp((int)vpAPts2D.Min(pt => pt.X), (int)vpBPts2D.Min(pt => pt.X), t);
                int RectMaxX = CameraUtil.Lerp((int)vpAPts2D.Max(pt => pt.X), (int)vpBPts2D.Max(pt => pt.X), t);
                int RectMinY = CameraUtil.Lerp((int)vpAPts2D.Min(pt => pt.Y), (int)vpBPts2D.Min(pt => pt.Y), t);
                int RectMaxY = CameraUtil.Lerp((int)vpAPts2D.Max(pt => pt.Y), (int)vpBPts2D.Max(pt => pt.Y), t);
                int w = Math.Max(1, RectMaxX - RectMinX);
                int h = Math.Max(1, RectMaxY - RectMinY);

                var Rect = new System.Drawing.Rectangle(RectMinX, RectMinY, w, h);
                avp.ZoomWindow(Rect);
            }
        REDRAW:
            rv.Redraw();
            CameraPath = new LineCurve(CamA, CamB);
        }

    }
}
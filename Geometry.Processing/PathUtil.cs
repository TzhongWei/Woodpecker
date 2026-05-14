using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Rhino;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;

namespace Woodpecker.Animation.Geometry.Processing
{
    public enum ReturnCurveOption
    {
        JointCurve,
        SplitCurve,
        OnlyAddedCurve
    }
    public class PathUtil
    {
        private int _dataCount => _finishedPathList.Count;
        private List<bool> _finishedPathList = new List<bool>();
        private List<Curve> _cPath;
        private bool _changeEnd;
        private PathUtil()
        {
            _finishedPathList = new List<bool>();
        }
        private PathUtil(int DataCount)
        {
            _finishedPathList = Enumerable.Repeat(false, DataCount).ToList();
        }
        public PathUtil(Curve cPath, bool ChangeEnd)
        {
            this._cPath = new List<Curve> { cPath };
            _changeEnd = ChangeEnd;
            _finishedPathList = Enumerable.Repeat(false, 1).ToList();
        }
        public PathUtil(List<Curve> cPaths, bool ChangeEnd)
        {
            this._cPath = new List<Curve>(cPaths);
            _changeEnd = ChangeEnd;
            _finishedPathList = Enumerable.Repeat(false, cPaths.Count).ToList();
        }
        public PathUtil Compute(double t, Vector3d Dir, double Distance, List<double> Speed, out Plane PL, out List<Curve> FinishPath, bool Followtangent)
        {
            var Tangent = new Vector3d();
            if (_dataCount == 1)
            {
                _drawpathCompute(0, this._cPath.First(), t, Dir, Distance, out PL, out var _ofinishPath, out Tangent);
                FinishPath = new List<Curve> { _ofinishPath };
            }
            else
            {
                _drawpathCompute(t, Dir, Distance, Speed, out PL, out FinishPath, out Tangent);
            }
            var projectTS = Transform.PlanarProjection(PL);
            Tangent.Transform(projectTS);
            if (Tangent.Unitize() && Followtangent)
            {
                var rot = Vector3d.VectorAngle(PL.XAxis, Tangent);
                PL.Rotate(rot, PL.ZAxis);
            }
            return this;
        }
        public PathUtil Compute(double t, Vector3d Dir, double Distance, List<double> Speed, out Plane PL, out List<Curve> FinishPath)
        {
            if (_dataCount == 1)
            {
                _drawpathCompute(0, this._cPath.First(), t, Dir, Distance, out PL, out var _ofinishPath, out _);
                FinishPath = new List<Curve> { _ofinishPath };
            }
            else
            {
                _drawpathCompute(t, Dir, Distance, Speed, out PL, out FinishPath, out _);
            }
            return this;
        }
        public PathUtil Compute(double t, Vector3d Dir, double Distance, out Plane PL, out List<Curve> FinishPath)
        {
            if (_dataCount == 1)
            {
                _drawpathCompute(0, this._cPath.First(), t, Dir, Distance, out PL, out var _ofinishPath, out _);
                FinishPath = new List<Curve> { _ofinishPath };
            }
            else
            {

                _drawpathCompute(t, Dir, Distance, null, out PL, out FinishPath, out _);
            }
            return this;
        }
        private bool _needRevsere = false;
        private void _drawpathCompute(double t, Vector3d Dir, double Distance, List<double> Speed, out Plane PL, out List<Curve> FinishPath, out Vector3d Tangent)
        {
            Tangent = new Vector3d();
            t = Math.Max(0.0, Math.Min(1, t));
            if (Speed == null || Speed.Count == 0)
            {
                Speed = Enumerable.Repeat(1.0, this._cPath.Count).ToList();
            }
            if (Speed.Count != _cPath.Count)
            {
                var _speed = new List<double>();
                for (int i = 0; i < _cPath.Count; i++)
                {
                    var ind = i > Speed.Count - 1 ? Speed.Count - 1 : i;
                    _speed.Add(Speed[ind]);
                }
                Speed = _speed;
            }
            if (Dir.IsZero || !Dir.IsValid)
                Dir = Vector3d.ZAxis;
            var cPaths = new List<Curve>(this._cPath);

            if (_needRevsere && _changeEnd)
            {
                cPaths.Reverse();
                Speed.Reverse();
            }


            if (t >= 1)
            {
                FinishPath = this._cPath;
                var lastPath = this._cPath.Last();
                var ptAtEnd = lastPath.PointAtEnd;
                PL = new Plane(ptAtEnd + Dir * Distance, Dir);
                _finishedPathList = Enumerable.Repeat(true, this._cPath.Count).ToList();
                _needRevsere = true;
                Tangent = lastPath.TangentAtEnd;
            }
            else if (t <= 1e-3)
            {
                FinishPath = null;
                var firstPath = _cPath.First();
                var ptAtSt = firstPath.PointAtStart;
                PL = new Plane(ptAtSt + Dir * Distance, Dir);
                _finishedPathList = Enumerable.Repeat(false, _cPath.Count).ToList();
                _needRevsere = false;
                Tangent = firstPath.TangentAtStart;
            }
            else
            {
                var sum = 0.0;
                var stateInterval = new List<Interval>();
                //Set up each curve time slot
                for (int i = 0; i < Speed.Count; i++)
                {
                    var oSum = sum;
                    sum += cPaths[i].Domain.Length / Speed[i];
                    stateInterval.Add(new Interval(oSum, sum));
                }
                var globalT = t * sum;
                var locInd = 0;
                for (int i = 0; i < stateInterval.Count; i++)
                {
                    if (stateInterval[i].IncludesParameter(globalT))
                    {
                        locInd = i;
                        break;
                    }
                }
                var cPath = cPaths[locInd];
                var Bag = new List<Curve>();

                var _t = (globalT - stateInterval[locInd].Min) / stateInterval[locInd].Length;

                _drawpathCompute(locInd, cPath, _t, Dir, Distance, out PL, out var _ofinishPath, out Tangent);

                for (int i = 0; i < locInd; i++)
                {
                    Bag.Add(cPaths[i]);
                    this._finishedPathList[locInd - 1] = true;
                }

                Bag.Add(_ofinishPath);
                FinishPath = Bag;
            }
        }
        private void _drawpathCompute(int Index, Curve cPath, double t, Vector3d Dir, double Distance, out Plane PL, out Curve FinishPath, out Vector3d Tangent)
        {
            if (!Dir.IsValid || Dir.IsZero)
            {
                Dir = Vector3d.ZAxis;
            }
            Interval dom = cPath.Domain;

            var ptAtT = Point3d.Unset;
            var _isfinished = this._finishedPathList[Index];
            if (t <= 1e-3)
            {
                ptAtT = cPath.PointAtStart;
                _isfinished = false;
                FinishPath = null;
                Tangent = cPath.TangentAtStart;

            }
            else if (t >= 1.0 - 1e-3)
            {
                ptAtT = cPath.PointAtEnd;
                _isfinished = true;
                FinishPath = cPath;
                Tangent = cPath.TangentAtEnd;
            }
            else
            {
                var getFirstSeg = true;
                //Split curve at tCurve and take the part from start ot t
                if (this._changeEnd && _isfinished)
                {
                    getFirstSeg = false;
                }
                var tCurve = getFirstSeg ? dom.T0 + t * dom.Length : dom.T1 - t * dom.Length;
                tCurve = Math.Max(dom.T0, Math.Min(dom.T1, tCurve));
                var pieces = cPath.Split(tCurve);
                ptAtT = cPath.PointAt(tCurve);
                //clamp to domain just in case
                if (pieces != null && pieces.Length > 0)
                {
                    // Take the piece that starts closest to the original domain start
                    var startSeg = getFirstSeg ? pieces.OrderBy(c => c.Domain.T0).First() : pieces.OrderBy(c => c.Domain.T0).Last();
                    FinishPath = startSeg;
                }
                else
                {
                    FinishPath = cPath;
                }
                Tangent = cPath.TangentAt(tCurve);
            }
            PL = new Plane(ptAtT + Dir * Distance, Dir);
            this._finishedPathList[Index] = _isfinished;
        }
        public static void DrawPaths(List<Curve> cPaths, double t, Vector3d Dir, double Distance, out Plane PL, out List<Curve> FinishPath, List<double> Speed = null)
        {
            if (cPaths == null || cPaths.Count == 0)
            {
                PL = new Plane();
                FinishPath = null;
                return;
            }
            t = Math.Max(0.0, Math.Min(1, t));

            if (Speed == null || Speed.Count == 0)
            {
                Speed = Enumerable.Repeat(1.0, cPaths.Count).ToList();
            }
            if (Speed.Count != cPaths.Count)
            {
                var _speed = new List<double>();
                for (int i = 0; i < cPaths.Count; i++)
                {
                    var ind = i > Speed.Count - 1 ? Speed.Count - 1 : i;
                    _speed.Add(Speed[ind]);
                }
                Speed = _speed;
            }

            if (t >= 1)
            {
                FinishPath = cPaths;
                var lastPath = cPaths.Last();
                var ptAtEnd = lastPath.PointAtEnd;
                PL = new Plane(ptAtEnd + Dir * Distance, Dir);
            }
            else if (t <= 1e-3)
            {
                FinishPath = null;
                var firstPath = cPaths.First();
                var ptAtSt = firstPath.PointAtStart;
                PL = new Plane(ptAtSt + Dir * Distance, Dir);
            }
            else
            {
                var sum = 0.0;
                var stateInterval = new List<Interval>();
                // Set up each curve time slot
                for (int i = 0; i < Speed.Count; i++)
                {
                    var oSum = sum;
                    sum += cPaths[i].Domain.Length / Speed[i];
                    stateInterval.Add(new Interval(oSum, sum));
                }

                var globalT = t * sum;
                var locInd = 0;
                for (int i = 0; i < stateInterval.Count; i++)
                {
                    if (stateInterval[i].IncludesParameter(globalT))
                    {
                        locInd = i;
                        break;
                    }
                }

                var cPath = cPaths[locInd];
                var Bag = new List<Curve>();
                for (int i = 0; i < locInd; i++)
                    Bag.Add(cPaths[i]);
                var _t = (globalT - stateInterval[locInd].Min) / stateInterval[locInd].Length;
                DrawPath(cPath, _t, Dir, Distance, out PL, out var _finishPath);
                Bag.Add(_finishPath);
                FinishPath = Bag;
            }
        }
        public static void DrawPath(Curve cPath, double t, Vector3d Dir, double Distance, out Plane PL, out Curve FinishPath)
        {
            Dir = !Dir.IsValid || Dir.IsZero ? Vector3d.ZAxis : Dir;
            var dom = cPath.Domain;
            var tCurve = dom.T0 + t * dom.Length;

            tCurve = Math.Max(dom.T0, Math.Min(dom.T1, tCurve));

            Point3d ptAt;

            if (t <= 1e-3)
            {
                ptAt = cPath.PointAtStart;
                FinishPath = null;
            }
            else if (t >= 1.0)
            {
                ptAt = cPath.PointAtEnd;
                FinishPath = cPath;
            }
            else
            {
                ptAt = cPath.PointAt(tCurve);

                var pieces = cPath.Split(tCurve);
                if (pieces != null && pieces.Length > 0)
                {
                    var startSeg = pieces.OrderBy(c => c.Domain.T0).First();
                    FinishPath = startSeg;
                }
                else
                    FinishPath = cPath;
            }
            PL = new Plane(ptAt + Dir * Distance, Dir);
        }
        public static List<Curve> AddEndedPath(Curve addCrv, List<double> Distances, List<Vector3d> Dirs, bool AddEnd, ReturnCurveOption returnCurve = ReturnCurveOption.SplitCurve)
        {
            if (Distances.Count == 1) Distances.Add(Distances[0]);

            var CrvPtAtSt = addCrv.PointAtStart;
            var tangentAtSt = addCrv.TangentAtStart;

            tangentAtSt.Reverse();
            if (Dirs.Count == 0 || Dirs == null)
            {
                Dirs.Add(tangentAtSt);
                var tangentAtEnd = addCrv.TangentAtEnd;
                Dirs.Add(tangentAtEnd);
            }

            Dirs = Dirs.Select(x => x == new Vector3d(0, 0, 0) || x == Vector3d.Unset ? tangentAtSt : x).ToList();
            if (Dirs.Count == 1) Dirs.Add(Dirs[0]);

            var crvLNAtSt = new LineCurve(CrvPtAtSt + Dirs[0] * Distances[0], CrvPtAtSt);
            Curve NewCrv = null;
            var Aggregate = new List<Curve>() { crvLNAtSt };
            if (addCrv.IsClosed)
            {
                NewCrv = addCrv.Split(addCrv.Domain.Max - RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)[0];
                addCrv = NewCrv;
                Aggregate.Add(addCrv);
            }
            else
            {
                Aggregate.Add(addCrv);
            }
            if (AddEnd)
            {
                var CrvPtAtEd = addCrv.PointAtEnd;
                var CrvLNAtEnd = new LineCurve(CrvPtAtEd, CrvPtAtEd + Dirs[1] * Distances[1]);
                Aggregate.Add(CrvLNAtEnd);
            }
            switch (returnCurve)
            {
                case ReturnCurveOption.JointCurve:
                    Aggregate = Curve.JoinCurves(Aggregate, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToList();
                    break;
                case ReturnCurveOption.SplitCurve:
                    break;
                case ReturnCurveOption.OnlyAddedCurve:
                    if (AddEnd)
                        Aggregate = new List<Curve> { Aggregate.First(), Aggregate.Last() };
                    else
                        Aggregate = new List<Curve> { Aggregate.First() };
                    break;
            }
            return Aggregate;
        }
        public static List<Curve> LinkPath(List<Curve> curves, List<int> ConnectPattern, ReturnCurveOption returnCurve = ReturnCurveOption.JointCurve)
        {
            var Segments = new List<Curve>();

            if (ConnectPattern.Count != curves.Count)
            {
                var newPattern = new List<int>();
                var ind = 0;
                for (int i = 0; i < curves.Count; i++)
                {
                    ind = ind > ConnectPattern.Count - 1 ? 0 : ind;
                    newPattern.Add(ConnectPattern[ind]);
                    ind++;
                }
                ConnectPattern = newPattern;
            }
            for (int i = 1; i < curves.Count; i++)
            {
                Point3d PtFirst, PtEnd, PtCan1, PtCan2;
                PtCan1 = curves[i].PointAtStart;
                PtCan2 = curves[i].PointAtEnd;
                switch (ConnectPattern[i - 1])
                {
                    case 1:
                        PtFirst = curves[i - 1].PointAtStart;
                        PtEnd = FindClosedPt(PtFirst, PtCan1, PtCan2);
                        Segments.Add(new LineCurve(PtFirst, PtEnd));
                        break;
                    case 2:
                        PtFirst = curves[i - 1].PointAtEnd;
                        PtEnd = FindClosedPt(PtFirst, PtCan1, PtCan2);
                        Segments.Add(new LineCurve(PtFirst, PtEnd));
                        break;
                    default:
                        break;
                }
            }

            switch (returnCurve)
            {
                case ReturnCurveOption.JointCurve:
                    Segments.AddRange(curves);
                    Segments = Curve.JoinCurves(Segments, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToList();
                    break;
                case ReturnCurveOption.SplitCurve:
                    var tempSeg = new List<Curve>();
                    for (int i = 0; i < curves.Count; i++)
                    {
                        tempSeg.Add(curves[i]);
                        if (i < curves.Count - 1)
                            tempSeg.Add(Segments[i]);
                    }
                    Segments = tempSeg;
                    break;
                case ReturnCurveOption.OnlyAddedCurve:
                    break;
            }
            return Segments;



            Point3d FindClosedPt(Point3d Target, Point3d Test1, Point3d Test2)
            {
                return Target.DistanceTo(Test1) > Target.DistanceTo(Test2) ? Test2 : Test1;
            }
        }
        public static Curve IterativeOffset(Curve curve, double Gap, double T = 0, int limit = 20, bool Direction = false, Plane plane = new Plane())
        {
            if (curve == null)
            {
                throw new Exception("input curve cannot be the null value");
            }
            if (plane == new Plane())
            {
                if (!curve.TryGetPlane(out plane))
                {
                    throw new Exception("curve isn't planar");
                }
            }
            Gap = OffsetDir(curve, plane, Gap * 0.1) ? -Gap : Gap;
            Gap = Direction ? Gap : -Gap;

            var offsetCrvs = new List<Curve>();
            var End = true;
            
            offsetCrvs.Add(curve);


            var CountL = 0;
            var OffVal = Gap;
            while (End && CountL < limit)
            {
                try
                {
                    var ReCrv = Curve.JoinCurves(curve.Offset(plane, OffVal, 1, CurveOffsetCornerStyle.Sharp));
                    if (ReCrv.Length > 1) throw new Exception("Create more than one offset Branches");
                    offsetCrvs.AddRange(ReCrv);
                }
                catch
                {
                    End = false;
                }
                CountL++;
                OffVal += Gap;
            }
            return ConnectLoopCrv(offsetCrvs, T);
        }
        private static bool OffsetDir(Curve crv, Plane PL, double Dist)
        {
            var pos = Curve.JoinCurves(crv.Offset(PL, Dist, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)).First();
            var posSrfArea = Brep.CreatePlanarBreps(pos, 0.1).First().GetArea();
            var neg = Curve.JoinCurves(crv.Offset(PL, -Dist, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp)).First();
            var negSrfArea = Brep.CreatePlanarBreps(neg, 0.1).First().GetArea();

            return negSrfArea < posSrfArea;
        }
        private static Curve ConnectLoopCrv(IEnumerable<Curve> Crvs, double t = 0)
        {
            var PtAtT = Crvs.First().PointAt(t);
            var NewCrvs = new List<Curve>();
            foreach (var Crv in Crvs)
            {
                Crv.ClosestPoint(PtAtT, out t);
                Crv.ChangeClosedCurveSeam(t);
                PtAtT = Crv.PointAt(t);
                var GDist = 0.1;
                var SphereCut = new Sphere(PtAtT, GDist).ToBrep();
                var Seg = Crv.Split(SphereCut, 1e-4, 1e-4).OrderBy(x => x.GetLength()).Last();

                NewCrvs.Add(Seg);

            }
            var Gap = new List<Curve>();
            for (int i = 0; i < NewCrvs.Count - 1; i++)
            {
                var LINE = new LineCurve(NewCrvs[i].PointAtEnd, NewCrvs[i + 1].PointAtStart);
                Gap.Add(LINE);
            }
            Gap.AddRange(NewCrvs);
            var Joints = Curve.JoinCurves(Gap);
            if (Joints.Length > 1) throw new Exception("Error for making a curve");
            return Joints.First();
        }
    }
}
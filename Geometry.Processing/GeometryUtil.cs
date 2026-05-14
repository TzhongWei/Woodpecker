using Rhino.Collections;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace Woodpecker.Animation.Geometry.Processing
{
    public static class GeometryUtil
    {
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
        public static bool CompareCrv(IEnumerable<Curve> CurveList1, IEnumerable<Curve> CurveList2)
        {
            if(CurveList1.Count() != CurveList2.Count())
                return false;
            else
            {
                var result = true;
                for(int i = 0; i < CurveList1.Count(); i ++)
                {
                    result &= CompareCrv(CurveList1.ToList()[i], CurveList2.ToList()[i]);
                }
                return result;
            }
        }
        public static bool CompareCrv(Curve Curve1, Curve Curve2)
        {
            if (!(Curve1.GetLength() == Curve2.GetLength()))
                return false;
            var DM1 = Curve1.Domain;
            var DM2 = Curve2.Domain;
            if (DM1 != DM2)
                return false;

            var ts = new List<double>();
            for (double i = DM1.Min; i < DM2.Max; i += 0.1)
                ts.Add(i);
            var pt1 = ts.Select(x => Curve1.PointAt(x));
            var pt2 = ts.Select(x => Curve2.PointAt(x));
            return pt1.SequenceEqual(pt2);
        }
    }
}

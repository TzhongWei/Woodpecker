using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryFromPlaneToPlane : TransformTSAction
    {
        public Plane From {get; private set;}
        public Plane To {get; private set;}
        public GeometryFromPlaneToPlane(string Name, double StartT, double EndT):base(Name, StartT, EndT)
        {
            From = new Plane();
            To = new Plane();
        }
        public GeometryFromPlaneToPlane(string Name, Plane From, Plane To, double StartT, double EndT):this(Name, StartT, EndT)
        {
            this.From = From;
            this.To = To;
        }
        public override Transform GetTransform(double NormalT)
        {
            var PtCur = GeometryUtil.Lerp(From.Origin, To.Origin, NormalT);
            var XCur = GeometryUtil.Lerp(From.XAxis, To.XAxis, NormalT);
            var YCur = GeometryUtil.Lerp(From.YAxis, To.YAxis, NormalT);
            var ZCur = GeometryUtil.Lerp(From.ZAxis, To.ZAxis, NormalT);
            var PLCur = new Plane(PtCur, XCur, YCur);
            return Transform.PlaneToPlane(this.From, PLCur);
        }
    }
}
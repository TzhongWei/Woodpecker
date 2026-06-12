using System.Collections.Generic;
using System.Drawing;
using Rhino.Geometry;
namespace Woodpecker.Animation.Geometry.Display
{
    public class DisplayVectorContent : DisplayContentAbstract<Vector3d>
    {
        private readonly Vector3d _vector;
        private readonly Point3d _anchor;
        private readonly Curve _vectorCrv;
        private readonly Point3d _arrowHeadLocation;
        private readonly VectorDisplaySetting _vectorDisplaySetting;
        public VectorDisplaySetting VectorDisplaySetting => _vectorDisplaySetting;
        public Point3d Anchor => _anchor;
        public List<Curve> VectorBody
        {
            get
            {
                if (this.dashType != DashType.Continuous)
                {
                    return new CurveDisplay(_vectorCrv, this.dashType).GetCurvesByDashType();
                }
                return new List<Curve>{_vectorCrv};
            }
        }
        public Point3d ArrowHeadLocation => _arrowHeadLocation;
        public DashType dashType = DashType.Continuous;
        public DisplayVectorContent(Point3d anchor, Vector3d vector):this(anchor, vector, new VectorDisplaySetting())
        {
        }
        
        
        public DisplayVectorContent(Point3d anchor, Vector3d vector, VectorDisplaySetting vectorDisplaySetting):base(vector, vectorDisplaySetting.Colour)
        {
            vector.Unitize();
            this._vector = vector;
            this._anchor = anchor;
            this._vectorDisplaySetting = vectorDisplaySetting;
            _vectorCrv = new LineCurve(this._anchor, anchor + vector * this.VectorDisplaySetting.Length);
            this._arrowHeadLocation = _vectorCrv.PointAtEnd;
            _clip = _vectorCrv.GetBoundingBox(true);
        }

        public DisplayVectorContent(Curve vectorCrv):this(vectorCrv, new VectorDisplaySetting())
        {
        }
        public DisplayVectorContent(Curve vectorCrv, VectorDisplaySetting vectorDisplaySetting):base(vectorCrv.TangentAtEnd, vectorDisplaySetting.Colour)
        {
            this._vectorCrv = vectorCrv;
            this._vector = vectorCrv.TangentAtEnd;
            this._anchor = vectorCrv.PointAtStart;
            this._vectorDisplaySetting = vectorDisplaySetting;
            this._arrowHeadLocation = vectorCrv.PointAtEnd;
            _clip = _vectorCrv.GetBoundingBox(true);
        }
        public Line GetLinearVectorDisplay()
        {
            return new Line(this.Anchor, this.ArrowHeadLocation);
        }
        private BoundingBox _clip;
        public override BoundingBox ClippingBox => _clip;
        public override bool IsValid => _vector.IsValid;
    }
}

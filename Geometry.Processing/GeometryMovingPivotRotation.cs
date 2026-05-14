using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryMovingPivotRotation : TransformTSAction
    {
        public Vector3d Axis { get; private set; }
        public double Angle { get; private set; }
        public Point3d Centre { get; private set; }

        public GeometryMovingPivotRotation(string Name, Vector3d Axis, double Angle, Point3d Centre, Interval timeline) : base(Name, timeline.Min, timeline.Max)
        {
            this.Axis = Axis;
            this.Angle = Angle;
            this.Centre = Centre;
        }

        public override Transform GetTransform(GeometryContent content, double NormalT)
        {
            var rotateCt = Centre;
            rotateCt.Transform(content.GetCurrentTransform());

            var finalAngle = Angle * NormalT;
            return Transform.Rotation(finalAngle, Axis, rotateCt);
        }
    }
}
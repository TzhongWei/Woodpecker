using System;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryTranslation : TransformTSAction
    {
        public Vector3d TranslateVector {get; private set;}
        public double Factor {get; private set;}
        public GeometryTranslation(string Name, double Start = 0, double End = 1):base(Name, Start, End)
        {
            TranslateVector = Vector3d.ZAxis;
        }
        public GeometryTranslation(string Name, Vector3d Translate, double Factor, Interval Timeline): this(Name, Translate, Factor, Timeline.Min, Timeline.Max){}
        public GeometryTranslation(string Name, Vector3d Translate, double Start = 0, double End = 1):this(Name, Start, End)
        {
            if(!Translate.Unitize())
                throw new Exception("Given vector is invalid");
            TranslateVector = Translate;
        }
        public GeometryTranslation(string Name, Vector3d Translate, double Factor, double Start, double End):this(Name, Translate, Start, End)
        {
            this.Factor = Factor;
        }
        public override Transform GetTransform(GeometryContent content, double NormalT)
        => TranslateVector == Vector3d.Zero ? Transform.Identity : Transform.Translation(TranslateVector * Factor * NormalT);
    }
}
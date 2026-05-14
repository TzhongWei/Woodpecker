using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryAnimationPipeline
    {
        private GeometryAnimation _geometryAnimation;
        public GeometryContent GeometryContent;
        public GeometryAnimationPipeline(GeometryAnimation AnimationSetting)
        {
            _geometryAnimation = AnimationSetting;
            _geometryAnimation.RemappingTimeline();
            GeometryContent = new GeometryContent(this, AnimationSetting.GetGeometry());
        }
        public List<string> Message => _geometryAnimation.Message;
        public bool Animate(double t)
        {

            t = Math.Max(0, Math.Min(1, t)); // Clamp t
            bool result = true;
            result &= _geometryAnimation.PreEvaluate(GeometryContent, t);
            if (result)
                result &= _geometryAnimation.Evaluate(GeometryContent, t);
            else
                return false;

            if (result)
                result &= _geometryAnimation.PostEvaluate(GeometryContent, t);

            return result;
        }
        public GeometryBase GeomObject => GeometryContent.GetCurrentGeometry().FirstOrDefault();
    }
}

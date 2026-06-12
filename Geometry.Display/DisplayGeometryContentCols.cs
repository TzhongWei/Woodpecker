using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.Geometry.Display
{
    public class DisplayGeometryContentCols : DisplayGeometryContent
    {
        private readonly List<Color> _colour;
        private readonly bool _linear;
        
        public DisplayGeometryContentCols(
            GeometryBase geometry,
            List<Color> colors,
            bool linear = true)
            : base(geometry, colors?.FirstOrDefault() ?? Color.Empty)
        {
            _colour = colors ?? new List<Color>();
            _linear = linear;
        }

        protected override Color GetColor()
        {
            if (_colour.Count == 0)
                return Color.Empty;

            var currentCol = t <= 1e-3
                ? _colour.First()
                : t >= 1 - 1e-3
                    ? _colour.Last()
                    : _linear
                        ? InterpolateLinear(t)
                        : InterpolateExponential(t);

            return Color.FromArgb(
                (int)Math.Round(currentCol.A * t),
                currentCol.R,
                currentCol.G,
                currentCol.B);
        }
        private Color InterpolateExponential(double t)
        {
            int count = _colour.Count;
            double scaledT = t * (count - 1);
            int index1 = (int)Math.Floor(scaledT);
            int index2 = (int)Math.Ceiling(scaledT);
            double localT = scaledT - index1;

            if (index2 >= count) index2 = count - 1;

            Color col1 = _colour[index1];
            Color col2 = _colour[index2];

            // Exponential interpolation
            double expT = TimelineSetting.Easing(localT);

            return DisplayUtil.LerpColor(col1, col2, expT);
        }
        private Color InterpolateLinear(double t)
        {
            int count = _colour.Count;
            double scaledT = t * (count - 1);
            int index1 = (int)Math.Floor(scaledT);
            int index2 = (int)Math.Ceiling(scaledT);
            double localT = scaledT - index1;

            if (index2 >= count) index2 = count - 1;

            Color col1 = _colour[index1];
            Color col2 = _colour[index2];

            return DisplayUtil.LerpColor(col1, col2, localT);
        }

    }
}

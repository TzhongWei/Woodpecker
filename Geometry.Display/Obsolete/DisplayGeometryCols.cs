using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper;
using Rhino.Geometry;
using System.Linq;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.Geometry.Display
{
    /// <summary>
    /// Display Geometry with multicols 
    /// </summary>
    [Obsolete]
    public class DisplayGeometryCols
    {
        private bool _linear;
        private List<Color> _iColors;
        private List<GeometryBase> _iGs;
        private BoundingBox _clip;
        private int _width;
        private double _pointer_t;
        public Color GetColor(double Pointer_t)
        {
            var currentCol = Pointer_t <= 1e-3 ? _iColors.First() : Pointer_t >= 1 - 1e-3 ? _iColors.Last() :
            _linear ? InterpolateLinear(Pointer_t) : InterpolateExponential(Pointer_t);
            return currentCol;
        }
        private Color InterpolateExponential(double t, TimelineSetting.EasingFunction easingFunction, params double[] easingParams)
        {
            int count = _iColors.Count;
            double scaledT = t * (count - 1);
            int index1 = (int)Math.Floor(scaledT);
            int index2 = (int)Math.Ceiling(scaledT);
            double localT = scaledT - index1;

            if (index2 >= count) index2 = count - 1;

            Color col1 = _iColors[index1];
            Color col2 = _iColors[index2];

            // Exponential interpolation
            double expT = easingFunction(localT, easingParams); // You can adjust the exponent for different effects

            return DisplayUtil.LerpColor(col1, col2, expT);
        }
        private Color InterpolateExponential(double t)
        {
            int count = _iColors.Count;
            double scaledT = t * (count - 1);
            int index1 = (int)Math.Floor(scaledT);
            int index2 = (int)Math.Ceiling(scaledT);
            double localT = scaledT - index1;

            if (index2 >= count) index2 = count - 1;

            Color col1 = _iColors[index1];
            Color col2 = _iColors[index2];

            // Exponential interpolation
            double expT = TimelineSetting.Easing(t); // You can adjust the exponent for different effects

            return DisplayUtil.LerpColor(col1, col2, expT);
        }
        private Color InterpolateLinear(double t)
        {
            int count = _iColors.Count;
            double scaledT = t * (count - 1);
            int index1 = (int)Math.Floor(scaledT);
            int index2 = (int)Math.Ceiling(scaledT);
            double localT = scaledT - index1;

            if (index2 >= count) index2 = count - 1;

            Color col1 = _iColors[index1];
            Color col2 = _iColors[index2];

            return DisplayUtil.LerpColor(col1, col2, t);
        }
        public DisplayGeometryCols(List<GeometryBase> iGs, List<Color> iColors, int Width, bool Linear = true)
        {
            _iGs = iGs;
            _iColors = iColors;
            _width = Width <= 0 ? 1 : Width;
            _linear = Linear;

            _clip = iGs.Aggregate(new BoundingBox(), (acc, g) => { acc.Union(g.GetBoundingBox(true)); return acc; });
        }
        public BoundingBox ClippingBox => _clip;
        public List<Color> GetColors() => _iColors;
        public List<GeometryBase> GetGeoms() => _iGs;
        public int GetWidth() => _width;
    }
}
using Rhino.Geometry;
using Rhino;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;

namespace Woodpecker.Animation.Geometry.Display
{
    public class CurveDisplay
    {
        public Curve O_Curve { get; private set; }
        private DashType _dash = "";
        public DashType GetDash() => _dash;
        private List<Curve> _displayCurves = new List<Curve>();
        public List<Curve> GetCurves() => _displayCurves;
        public List<Curve> GetCurvesByDashType()
        {
            var patterns = _dash.GetPattern();

            if(_dash.ScalebyScreen)
            {
                var _crvs = ApplyDashPattern(this.O_Curve, patterns).ToList();
                return _crvs;
            }
            else
            return _displayCurves;

        }
        public CurveDisplay(Curve curve, string dash = "")
        {
            this.O_Curve = curve;
            this._dash = dash;
            
            if(string.IsNullOrWhiteSpace(dash))
            {
                _displayCurves.Add(curve);
            }
            else
            {
                var pattern = ParseDashPattern(dash);
                _displayCurves.AddRange(ApplyDashPattern(curve, pattern));
            }

        }
        public CurveDisplay(Curve curve, DashType dashType)
        {
            this.O_Curve = curve;
            this._dash = dashType;
            if(dashType.PatternPixel == new double[]{})
            {
                _displayCurves.Add(curve);
            }
            else
            {
                _displayCurves.AddRange(ApplyDashPattern(curve, dashType.PatternPixel));
            }
            
        }
        private IEnumerable<Curve> ApplyDashPattern(Curve curve, double[] pattern)
        {
            if (pattern == null || pattern.Length == 0)
                return new Curve[] { curve };

            double curveLength = curve.GetLength();
            List<Curve> dashes = new List<Curve>();

            double offset0 = 0.0;
            int index = 0;
            while (true)
            {
                // Get the current dash length.
                double dashLength = pattern[index++];
                if (index >= pattern.Length)
                    index = 0;

                // Compute the offset of the current dash from the curve start.
                double offset1 = offset0 + dashLength;
                if (offset1 > curveLength)
                    offset1 = curveLength;

                // Solve the curve parameters at the current dash start and end.
                double t0, t1;
                curve.LengthParameter(offset0, out t0);
                curve.LengthParameter(offset1, out t1);

                Curve dash = curve.Trim(t0, t1);
                if (dash != null)
                    dashes.Add(dash);

                // Get the current gap length.
                double gapLength = pattern[index++];
                if (index >= pattern.Length)
                    index = 0;

                // Set the start of the next dash to be the end of the current
                // dash + the length of the adjacent gap.
                offset0 = offset1 + gapLength;

                // Abort when we've reached the end of the curve.
                if (offset0 >= curveLength)
                    break;
            }

            return dashes;
        }
        private double[] ParseDashPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return null;

            string[] fragments = pattern.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (fragments == null || fragments.Length == 0)
                return null;

            double[] values = new double[fragments.Length];
            for (int i = 0; i < fragments.Length; i++)
            {
                double v;
                if (!double.TryParse(fragments[i], NumberStyles.Float, CultureInfo.InstalledUICulture, out v))
                    throw new Exception(fragments[i] + " is a not a valid number.");

                if (v <= 0.0)
                    throw new Exception("Dashes or gaps must have a strictly positive length.");

                values[i] = v;
            }

            return values;
        }

    }
}
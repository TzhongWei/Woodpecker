using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Grasshopper.Kernel.Expressions;
using Rhino;
using Rhino.Geometry;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.Geometry.Display
{
    public class DashType
    {
        public bool ScalebyScreen = false;
        public DashType(string name, IEnumerable<double> pattern)
        {
            Name = name;
            PatternPixel = pattern.ToArray();
        }
        public DashType(string name, double[] pattern)
        {
            Name = name;
            PatternPixel = pattern;
        }
        public string Name { get; set; }
        public double[] PatternPixel { get; set; }
        public double[] GetPattern()
        {
            if (ScalebyScreen)
                try
                {
                    RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetWorldToScreenScale(Point3d.Origin, out var pixelsPerUnit);
                    if (pixelsPerUnit <= 0 || double.IsNaN(pixelsPerUnit) || double.IsInfinity(pixelsPerUnit))
                        pixelsPerUnit = 1.0;

                    // CurveDisplay expects dash values in model units, not pixels.
                    // PatternPixel is stored in screen pixels, so convert it back to model units here.
                    var patternInModelUnits = PatternPixel.Select(x => x / pixelsPerUnit);
                    return patternInModelUnits.Select(FormatDashValue).ToArray();
                }
                catch
                {
                    return PatternPixel.Select(FormatDashValue).ToArray();
                }
            else
            {
                return PatternPixel.Select(FormatDashValue).ToArray();
            }
        }
        private static double FormatDashValue(double value)
        {
            var rounded = Math.Round(value, 3);
            if (rounded <= 0.0)
                rounded = 0.001;
            return rounded;
        }
        // Default dash types
        public static readonly DashType Continuous = new DashType("Continuous", new double[] { });
        public static readonly DashType Dot = new DashType("Dot", new double[] { 2, 4 });
        public static readonly DashType Dashed = new DashType("Dashed", new double[] { 4, 4 });
        public static readonly DashType DashDot = new DashType("DashDot", new double[] { 6, 6, 1, 6 });
        public static readonly DashType Hidden = new DashType("Hidden", new double[] { 6, 6 });
        public override string ToString()
        {
            return $"{Name}: {string.Join(" ", PatternPixel)} px\n Scale by Screen: {ScalebyScreen}";
        }
        public static implicit operator DashType(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Continuous;

            var name = "UnDefined";
            var pattern = new List<double>();
            var scaleByScreen = false;

            var lines = text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (lines.Count == 0)
                return Continuous;

            foreach (var line in lines)
            {
                if (line.StartsWith("Scale by Screen", StringComparison.OrdinalIgnoreCase))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    if (split.Length == 2)
                        bool.TryParse(split[1].Trim(), out scaleByScreen);
                    continue;
                }

                var patternText = line;
                var namePatternSplit = line.Split(new[] { ':' }, 2);
                if (namePatternSplit.Length == 2)
                {
                    name = string.IsNullOrWhiteSpace(namePatternSplit[0])
                        ? "UnDefined"
                        : namePatternSplit[0].Trim();
                    patternText = namePatternSplit[1];
                }

                patternText = patternText
                    .Replace("px", "")
                    .Replace(",", " ")
                    .Replace(";", " ");

                foreach (var token in patternText.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
                        double.TryParse(token, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                    {
                        if (value > 0 && !double.IsNaN(value) && !double.IsInfinity(value))
                            pattern.Add(value);
                    }
                }
            }

            return new DashType(name, pattern)
            {
                ScalebyScreen = scaleByScreen
            };
        }
    }
}
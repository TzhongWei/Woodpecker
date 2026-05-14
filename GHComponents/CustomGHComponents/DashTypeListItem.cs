using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public class DashTypeListItem : ValueListItem
    {
        public bool ScalebyScreen = false;
        public readonly DashType DashPattern;
        public DashTypeListItem(): base(){} 
        public DashTypeListItem(string name, string patternString, bool ScaleByView) : base(name, patternString)
        {
            DashPattern = new DashType(name, ParsePattern(patternString));
            DashPattern.ScalebyScreen = ScaleByView;
            this.ScalebyScreen = ScaleByView;
        }
        public static List<DashTypeListItem> SetDashTypeList(DashCodeParam CodeParam)
        {
            var result = new List<DashTypeListItem>();
            foreach(var code in CodeParam)
            {
                var name = code.Name;
                var pattern = string.Join(" ", code.PatternPixel.Select(x => x.ToString()));
                result.Add(new DashTypeListItem(name, pattern, code.ScalebyScreen));
            }
            return result;
        }
        public override ValueListItem Clone()
        {
            var item = new DashTypeListItem(Name, Expression, ScalebyScreen);
            item.Selected = Selected;
            return item;
        }
        public DashTypeListItem CloneDashType() => this.Clone() as DashTypeListItem;
        private static IEnumerable<double> ParsePattern(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return Enumerable.Empty<double>();

            return expression
                .Split(new[] { ' ', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    if (!double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                        throw new System.Exception("The pattern string format error");

                    return d;
                })
                .ToArray();
        }
    }
}

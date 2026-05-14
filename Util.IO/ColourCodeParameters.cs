using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace Woodpecker.Animation.Util.IO
{
    public class ColourCodeParameters : CodeParameters<Color>
    {
        public ColourCodeParameters(Dictionary<string, List<Color>> values) : base(values)
        { }
        public ColourCodeParameters(Dictionary<string, List<string>> value) : base()
        {
            this._values = ColourCodeUtil.StringToColourDictionary(value);
        }
        public override DataTree<string> To_GH_DataTree()
        {
            var dic = this.Values;
            var tree = new DataTree<string>();
            int ind = 0;
            foreach (var kvp in this.Values)
            {
                tree.Add(kvp.Key, new GH_Path(ind));
                tree.AddRange(kvp.Value.Select(x => ColourCodeUtil.ParseColourCss(x)).ToArray(), new GH_Path(ind));
                ind++;
            }
            return tree;
        }
    }
}
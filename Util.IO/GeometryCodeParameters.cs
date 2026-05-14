using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.UI.Controls;

namespace Woodpecker.Animation.Util.IO
{
    public class GeometryCodeParameters: CodeParameters<GeometryDataPair>
    {
        public GeometryCodeParameters(Dictionary<string, List<GeometryDataPair>> values) : base(values)
        {
            
        }
        public GeometryCodeParameters(Dictionary<string, List<GeometryBase>> values):base()
        {
            var Params = new Dictionary<string, List<GeometryDataPair>>();
            foreach(var kvp in values)
            {
                Params.Add(kvp.Key, kvp.Value.Select(x =>  GeometryCodeUtil.CreateGeometryDataPair(x)).ToList());
            }
            this._values = Params;
        }
        public Dictionary<string, List<GeometryBase>> GeomValues 
        {
            get
            {
                var Dic = new Dictionary<string, List<GeometryBase>>();
                foreach(var kvp in this._values)
                {
                    Dic[kvp.Key] = kvp.Value.Select(x => (GeometryBase)x ).ToList();

                }
                return Dic;
            }

        }
        public override DataTree<string> To_GH_DataTree()
        {
            var geometryTree = new DataTree<string>();
            var ind = 0;
            foreach(var kvp in this._values)
            {
                geometryTree.Add(kvp.Key, new GH_Path(ind));
                geometryTree.AddRange(kvp.Value.SelectMany(x => (List<string>)(GeometryDataPair)x), new GH_Path(ind));
                ind++;
            }
            return geometryTree;
        }
    }
}
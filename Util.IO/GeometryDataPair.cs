using System.Collections.Generic;
using Rhino.Geometry;

namespace Woodpecker.Animation.Util.IO
{
    public class GeometryDataPair
    {
        public GeometryDataPair() { }
        public string GeometryType { get; set; }
        public string GeometryDataJson { get; set; }
        public static implicit operator GeometryDataPair(GeometryBase Geom)
            => GeometryCodeUtil.CreateGeometryDataPair(Geom);
        public static implicit operator GeometryBase(GeometryDataPair pair)
            => GeometryCodeUtil.ReadGeometryDataPair(pair);
        public static implicit operator List<string>(GeometryDataPair pair)
            => new List<string>{pair.GeometryType, pair.GeometryDataJson};
        
    }
}
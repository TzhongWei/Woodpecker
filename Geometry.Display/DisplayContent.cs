using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace Woodpecker.Geometry.Display
{
    public class DisplayContent
    {
        public readonly GeometryBase Geometry;
        public string Type => Geometry.GetType().Name;
        public List<Curve> GeometryWireframe => _geometrywire;
        private List<Curve> _geometrywire;
        public Dictionary<string, object> Attributes;
        public readonly Color Colour;
        public DisplayContent(GeometryBase Geometry, Color colour)
        {
            this.Geometry = Geometry;
            this.Colour = colour;
        }
    }
}

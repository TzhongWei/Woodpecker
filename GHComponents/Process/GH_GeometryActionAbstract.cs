using Grasshopper.Kernel;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Geometry Action Abstract component.
    /// </summary>
    public abstract class GH_GeometryActionAbstract : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_GeometryActionAbstract(string Name, string Nickname, string Description ): 
        base(Name, Nickname, Description, "Woodpecker", "Process"){}
        protected abstract Geometry.Processing.GeometryActionAbstract _geometryActionAbstract {get; set;}
        public override string ToString()
        {
            return _geometryActionAbstract.Name;
        }
    }
}
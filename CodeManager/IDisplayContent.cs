using System.Collections.Generic;
using System.Drawing;
using Rhino.Geometry;


namespace Woodpecker.Animation.CodeManager
{
    public interface IDisplayContent
    {
        BoundingBox ClippingBox {get;}
        bool IsValid {get;}
        bool Visible {get;}
    }
}
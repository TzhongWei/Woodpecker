using Rhino.Geometry;
using Rhino.DocObjects;
namespace Woodpecker.Animation.Geometry.Display
{
    public enum RenderTagMode
    {
        OnPlane,
        OnWindow,
    }
    public class AnnotationDisplaySetting
    {
        public AnnotationDisplaySetting()
        {
            this.TagMode = RenderTagMode.OnWindow;
            this.Height = 14;
            this.FontFace = "Arial";
            this.MiddleJustified = true;
            this.Bold = false;
            this.Italic = false;
            this.HorizontalAlignment = TextHorizontalAlignment.Center;
            this.VerticalAlignment = TextVerticalAlignment.Middle;
            this.Displayplane = Plane.WorldXY;
        }
        public RenderTagMode TagMode = RenderTagMode.OnWindow;
        public Plane Displayplane {get; set;}= Plane.WorldXY;
        private int _height = 14;
        public int Height
        {
            get => _height;
            set => _height = value > 0 ? value : 14;
        }
        public string FontFace { get; set; } = "Arial";
        public bool MiddleJustified { get; set; } = true;
        public bool Bold {get; set;} = false;
        public bool Italic {get; set;} = false;
        public Rhino.DocObjects.TextHorizontalAlignment HorizontalAlignment { get; set; } = Rhino.DocObjects.TextHorizontalAlignment.Center;
        public Rhino.DocObjects.TextVerticalAlignment VerticalAlignment { get; set; } = Rhino.DocObjects.TextVerticalAlignment.Middle;

    }
}
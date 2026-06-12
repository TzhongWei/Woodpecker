using System.Drawing;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Display
{
    public sealed class DisplayAnnotationContent
        : DisplayContentAbstract<string>
    {
        private readonly BoundingBox _clippingBox;
        public Plane TextPlane {get
            {
                var pl = annotationDisplaySetting.Displayplane;
                pl.Origin = this.Location;
                return pl;
            }
        }
        public DisplayAnnotationContent(
            string text,
            Point3d location,
            Color colour, 
            AnnotationDisplaySetting annotationDisplaySetting)
            : base(text, colour)
        {
            Location = location;
            this.annotationDisplaySetting =
                annotationDisplaySetting ?? new AnnotationDisplaySetting();
            _clippingBox = CreateClippingBox();
        }

        public Point3d Location { get; }
        public readonly AnnotationDisplaySetting annotationDisplaySetting;

        public override bool IsValid =>
            !string.IsNullOrWhiteSpace(DisplayObject) &&
            Location.IsValid;

        public override BoundingBox ClippingBox => _clippingBox;

        private BoundingBox CreateClippingBox()
        {
            if (!Location.IsValid)
                return BoundingBox.Empty;

            if (annotationDisplaySetting.TagMode == RenderTagMode.OnWindow ||
                string.IsNullOrWhiteSpace(DisplayObject))
            {
                return new BoundingBox(Location, Location);
            }

            var style = new DimensionStyle
            {
                TextHeight = annotationDisplaySetting.Height,
                Font = Rhino.DocObjects.Font.FromQuartetProperties(
                    annotationDisplaySetting.FontFace,
                    annotationDisplaySetting.Bold,
                    annotationDisplaySetting.Italic)
            };

            var textEntity = TextEntity.Create(
                DisplayObject,
                TextPlane,
                style,
                false,
                0.0,
                0.0);

            if (textEntity == null)
                return new BoundingBox(Location, Location);

            textEntity.TextHorizontalAlignment =
                annotationDisplaySetting.HorizontalAlignment;
            textEntity.TextVerticalAlignment =
                annotationDisplaySetting.VerticalAlignment;

            var textBounds = textEntity.GetBoundingBox(true);
            return textBounds.IsValid
                ? textBounds
                : new BoundingBox(Location, Location);
        }
    }
}

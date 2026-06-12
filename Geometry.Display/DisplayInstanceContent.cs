using System.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Display
{
    public sealed class DisplayInstanceContent
        : DisplayContentAbstract<InstanceReferenceGeometry>
    {
        private readonly RhinoDoc _document;
        private readonly BoundingBox _clippingBox;

        public DisplayInstanceContent(
            InstanceReferenceGeometry instance,
            RhinoDoc document)
            : base(instance, Color.White)
        {
            _document = document;
            Definition = ResolveDefinition();
            _clippingBox = CalculateClippingBox();
        }

        public InstanceDefinition Definition { get; }

        public Transform InstanceTransform =>
            DisplayObject?.Xform ?? Transform.Unset;

        public override bool IsValid =>
            DisplayObject != null &&
            DisplayObject.IsValid &&
            Definition != null &&
            !Definition.IsDeleted;

        public override BoundingBox ClippingBox => _clippingBox;

        private InstanceDefinition ResolveDefinition()
        {
            if (_document == null || DisplayObject == null)
                return null;

            return _document.InstanceDefinitions.Find(
                DisplayObject.ParentIdefId,
                true);
        }

        private BoundingBox CalculateClippingBox()
        {
            if (Definition == null || DisplayObject == null)
                return BoundingBox.Empty;

            var result = BoundingBox.Empty;

            foreach (var definitionObject in Definition.GetObjects())
            {
                var geometry = definitionObject?.Geometry;
                if (geometry == null || !geometry.IsValid)
                    continue;

                var objectBox = geometry.GetBoundingBox(true);
                if (!objectBox.IsValid)
                    continue;

                objectBox.Transform(DisplayObject.Xform);
                result.Union(objectBox);
            }

            return result;
        }
    }
}

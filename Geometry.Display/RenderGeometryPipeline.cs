using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;

namespace Woodpecker.Animation.Geometry.Display
{
    public enum GeometryRenderMode
    {
        Shaded,
        Wire,
    }

    public sealed class RenderGeometryPipeline
        : RenderPipelineAbstract<DisplayGeometryContent>
    {
        public GeometryRenderMode RenderMode { get; set; }
        public Color? OverrideColour { get; set; }

        public RenderGeometryPipeline(
            IEnumerable<DisplayGeometryContent> contents, GH_Component Component,
            GeometryRenderMode renderMode = GeometryRenderMode.Shaded)
            : base(contents, Component)
        {
            RenderMode = renderMode;
        }

        public override void Render(DisplayPipeline display)
        {
            if (display == null)
                return;

            foreach (var content in m_Contents)
            {
                if (content == null || !content.Visible || !content.IsValid)
                    continue;

                var colour = OverrideColour ?? content.DisplayColour;
                colour = m_component.Attributes.Selected ? DisplayDefaultColour.SelectedColour : colour;

                if (colour.A <= 0)
                    continue;
                if (content.IsPoint)
                {
                    display.DrawPoint(((Rhino.Geometry.Point)content.DisplayObject).Location, colour);
                }

                if (RenderMode == GeometryRenderMode.Shaded)
                {
                    var material = new DisplayMaterial(
                        colour,
                        content.transparency);

                    foreach (var mesh in content.GeometryMesh)
                    {
                        if (mesh != null && mesh.IsValid)
                            display.DrawMeshShaded(mesh, material);
                    }
                }
                else
                {
                    foreach (var curve in content.GeometryWireFrame)
                    {
                        if (curve != null && curve.IsValid)
                            display.DrawCurve(curve, colour, content.Width);
                    }
                }
            }
        }
    }
}

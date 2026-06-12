using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;

namespace Woodpecker.Animation.Geometry.Display
{
    /// <summary>
    /// Applies temporary GPU clipping planes only while the target pipeline
    /// renders, then removes them before the remaining display pipeline runs.
    /// </summary>
    public sealed class RenderClippingPipeline : RenderPipelineAbstract<DisplayClippingContent>
    {
        public GeometryRenderMode RenderMode { get; set; }
        public Color? OverrideColour { get; set; }

        public RenderClippingPipeline(
            IEnumerable<DisplayClippingContent> contents,
            GH_Component component,
            GeometryRenderMode geometryRender = GeometryRenderMode.Shaded)
            :
            base(contents, component)
        {
            RenderMode = geometryRender;
        }

        public override void Render(DisplayPipeline display)
        {
            if (display == null || !ShouldRender)
                return;

            display.EnableClippingPlanes(true);

            foreach (var content in m_Contents)
            {
                if (content == null ||
                    !content.Visible ||
                    !content.IsValid)
                    continue;

                RenderContent(display, content);
            }
        }

        private void RenderContent(
            DisplayPipeline display,
            DisplayClippingContent content)
        {
            var clipIndices = new List<int>();

            try
            {
                foreach (var plane in content.ClippingPlanes)
                {
                    var index = display.AddClippingPlane(
                        plane.Origin,
                        plane.Normal);

                    if (index >= 0)
                        clipIndices.Add(index);
                }

                if (clipIndices.Count == 0)
                    return;

                var colour = OverrideColour ?? content.DisplayColour;
                if (m_component?.Attributes?.Selected == true)
                    colour = DisplayDefaultColour.SelectedColour;

                if (colour.A <= 0)
                    return;

                if (content.IsPoint)
                {
                    display.DrawPoint(
                        ((Rhino.Geometry.Point)content.DisplayObject).Location,
                        colour);
                    return;
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

                    if (content.ShowSectionFill)
                    {
                        RemoveClippingPlanes(
                            display,
                            clipIndices);

                        RenderSectionFills(
                            display,
                            content,
                            material);
                    }
                }
                else
                {
                    foreach (var curve in content.GeometryWireFrame)
                    {
                        if (curve != null && curve.IsValid)
                            display.DrawCurve(curve, colour, content.Width);
                    }

                    if (content.ShowSectionWire)
                    {
                        RemoveClippingPlanes(
                            display,
                            clipIndices);

                        RenderSectionWires(
                            display,
                            content,
                            colour);
                    }
                }
            }
            finally
            {
                RemoveClippingPlanes(
                    display,
                    clipIndices);
            }
        }

        private static void RenderSectionWires(
            DisplayPipeline display,
            DisplayClippingContent content,
            Color colour)
        {
            for (var sectionPlaneIndex = 0;
                sectionPlaneIndex <
                    content.ClippingPlanes.Count;
                sectionPlaneIndex++)
            {
                var sectionCurves =
                    content.GetSectionCurves(
                        sectionPlaneIndex);

                if (sectionCurves.Count == 0)
                    continue;

                var clipIndices = AddOtherClippingPlanes(
                    display,
                    content,
                    sectionPlaneIndex);

                try
                {
                    foreach (var sectionCurve in
                        sectionCurves)
                    {
                        if (sectionCurve != null &&
                            sectionCurve.IsValid)
                        {
                            display.DrawCurve(
                                sectionCurve,
                                colour,
                                content.Width);
                        }
                    }
                }
                finally
                {
                    RemoveClippingPlanes(
                        display,
                        clipIndices);
                }
            }
        }

        private static void RenderSectionFills(
            DisplayPipeline display,
            DisplayClippingContent content,
            DisplayMaterial material)
        {
            for (var sectionPlaneIndex = 0;
                sectionPlaneIndex <
                    content.ClippingPlanes.Count;
                sectionPlaneIndex++)
            {
                var sectionMeshes =
                    content.GetSectionFillMeshes(
                        sectionPlaneIndex);

                if (sectionMeshes.Count == 0)
                    continue;

                var clipIndices = AddOtherClippingPlanes(
                    display,
                    content,
                    sectionPlaneIndex);

                try
                {
                    foreach (var sectionMesh in
                        sectionMeshes)
                    {
                        if (sectionMesh != null &&
                            sectionMesh.IsValid)
                        {
                            display.DrawMeshShaded(
                                sectionMesh,
                                material);
                        }
                    }
                }
                finally
                {
                    RemoveClippingPlanes(
                        display,
                        clipIndices);
                }
            }
        }

        private static List<int> AddOtherClippingPlanes(
            DisplayPipeline display,
            DisplayClippingContent content,
            int sourcePlaneIndex)
        {
            var clipIndices = new List<int>();

            for (var clipPlaneIndex = 0;
                clipPlaneIndex <
                    content.ClippingPlanes.Count;
                clipPlaneIndex++)
            {
                // Section geometry lies on its source plane. Only the
                // remaining planes should trim it.
                if (clipPlaneIndex == sourcePlaneIndex)
                    continue;

                var plane =
                    content.ClippingPlanes[
                        clipPlaneIndex];
                var index =
                    display.AddClippingPlane(
                        plane.Origin,
                        plane.Normal);

                if (index >= 0)
                    clipIndices.Add(index);
            }

            return clipIndices;
        }

        private static void RemoveClippingPlanes(
            DisplayPipeline display,
            List<int> clipIndices)
        {
            for (var i = clipIndices.Count - 1; i >= 0; i--)
                display.RemoveClippingPlane(clipIndices[i]);

            clipIndices.Clear();
        }
    }
}

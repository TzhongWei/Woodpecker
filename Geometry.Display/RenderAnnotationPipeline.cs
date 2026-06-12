using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Display;

namespace Woodpecker.Animation.Geometry.Display
{
    public sealed class RenderAnnotationPipeline
        : RenderPipelineAbstract<DisplayAnnotationContent>
    {
        public RenderAnnotationPipeline(
            IEnumerable<DisplayAnnotationContent> contents, GH_Component Component)
            : base(contents, Component)
        {
            Stage = RenderStage.Foreground;
        }

        public override void Render(DisplayPipeline display)
        {
            if (display == null)
                return;

            foreach (var content in m_Contents)
            {
                if (content == null ||
                    !content.Visible ||
                    !content.IsValid)
                {
                    continue;
                }

                var colour = content.DisplayColour;
                colour = m_component.Attributes.Selected ? DisplayDefaultColour.SelectedColour : colour;

                if (colour.A <= 0)
                    continue;

                if (RenderTagMode.OnWindow == content.annotationDisplaySetting.TagMode)
                    display.Draw2dText(
                       content.DisplayObject,
                       colour,
                       content.Location,
                       content.annotationDisplaySetting.MiddleJustified,
                       content.annotationDisplaySetting.Height,
                       content.annotationDisplaySetting.FontFace);
                else
                {
                    display.Draw3dText(
                        content.DisplayObject,
                        colour,
                        content.TextPlane,
                        content.annotationDisplaySetting.Height,
                        content.annotationDisplaySetting.FontFace,
                        content.annotationDisplaySetting.Bold,
                        content.annotationDisplaySetting.Italic,
                        content.annotationDisplaySetting.HorizontalAlignment,
                        content.annotationDisplaySetting.VerticalAlignment
                        );
                }
            }
        }
    }
}

using System;
using System.Drawing;
using System.Collections.Generic;
using Rhino.Display;
using Grasshopper.Kernel;

namespace Woodpecker.Animation.Geometry.Display
{

    public enum VectorRenderMode
    {
        Linear,
        Curved
    }
    public sealed class RenderVectorPipeline : RenderPipelineAbstract<DisplayVectorContent>
    {
        public VectorRenderMode RenderMode { get; set; }
        public Color? OverrideColour { get; set; }
        public RenderVectorPipeline(
            IEnumerable<DisplayVectorContent> contents,
            GH_Component component,
        VectorRenderMode renderMode = VectorRenderMode.Linear
        ) : base(contents, component)
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
                var colour = m_component.Attributes.Selected ? DisplayDefaultColour.SelectedColour : content.DisplayColour;
                if(colour.A <= 0)
                    continue;
                if (RenderMode == VectorRenderMode.Linear && content.dashType == DashType.Continuous)
                {
                    display.DrawArrow(
                        content.GetLinearVectorDisplay(),
                        colour,
                        content.VectorDisplaySetting.ArrowheadSize,
                        content.VectorDisplaySetting.ArrowRelativeSize
                    );
                }
                else
                {
                    display.DrawArrowHead(
                        content.ArrowHeadLocation,
                        content.DisplayObject,
                        colour,
                        content.VectorDisplaySetting.ArrowheadSize,
                        content.VectorDisplaySetting.ArrowRelativeSize
                    );
                    foreach (var crv in content.VectorBody)
                        display.DrawCurve(
                        crv,
                        colour,
                        content.VectorDisplaySetting.Width
                    );
                }
            }
        }
    }
}
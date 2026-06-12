using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Display;

namespace Woodpecker.Animation.Geometry.Display
{
    public sealed class RenderInstancePipeline
        : RenderPipelineAbstract<DisplayInstanceContent>
    {
        public RenderInstancePipeline(
            IEnumerable<DisplayInstanceContent> contents, GH_Component component)
            : base(contents, component)
        {
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

                display.DrawInstanceDefinition(
                    content.Definition,
                    content.InstanceTransform);
            }
        }
    }
}

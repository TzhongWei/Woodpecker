using Rhino.Display;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Display
{
    public sealed class DisplayGeometryConduit : DisplayConduit
    {
        private readonly List<IRenderPipeline> _pipelines;

        public DisplayGeometryConduit()
        {
            _pipelines = new List<IRenderPipeline>();
        }

        public void Register(IRenderPipeline pipeline)
        {
            if (pipeline != null && !_pipelines.Contains(pipeline))
                _pipelines.Add(pipeline);
        }

        public bool Unregister(IRenderPipeline pipeline)
        {
            return pipeline != null && _pipelines.Remove(pipeline);
        }

        public void Clear()
        {
            _pipelines.Clear();
        }

        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            base.CalculateBoundingBox(e);

            foreach (var pipeline in _pipelines)
            {
                if (!pipeline.ShouldRender)
                    continue;

                var clippingBox = pipeline.ClippingBox;
                if (clippingBox.IsValid)
                    e.IncludeBoundingBox(clippingBox);
            }
        }

        protected override void PreDrawObjects(DrawEventArgs e)
            => RenderStage(e.Display, Woodpecker.Animation.Geometry.Display.RenderStage.PreDrawObjects);

        protected override void PostDrawObjects(DrawEventArgs e)
            => RenderStage(e.Display, Woodpecker.Animation.Geometry.Display.RenderStage.PostDrawObjects);

        protected override void DrawForeground(DrawEventArgs e)
            => RenderStage(e.Display, Woodpecker.Animation.Geometry.Display.RenderStage.Foreground);

        private void RenderStage(DisplayPipeline display, Woodpecker.Animation.Geometry.Display.RenderStage stage)
        {
            foreach (var pipeline in _pipelines)
            {
                if (pipeline.ShouldRender && pipeline.Stage == stage)
                    pipeline.Render(display);
            }
        }
    }
}

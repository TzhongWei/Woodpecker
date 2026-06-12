using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Need debugs
    /// </summary>
    public sealed class GH_DisplayAnnotation : GH_DisplayGeometryAbstract
    {
        private readonly RenderAnnotationPipeline _pipeline;
        protected override IRenderPipeline renderPipeline => _pipeline;

        public GH_DisplayAnnotation()
            : base(
                "Display Annotation",
                "DispText",
                "Display screen-facing text annotations at world-space points.")
        {
            _pipeline = new RenderAnnotationPipeline(
                new List<DisplayAnnotationContent>(), this);

            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_pipeline);
            _conduit.Enabled = true;
        }

        public override Guid ComponentGuid =>
            new Guid("f2a8e360-d607-40e5-9b86-4fa538bfa322");

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddTextParameter(
                "Text",
                "T",
                "Annotation text.",
                GH_ParamAccess.list);

            pManager.AddPointParameter(
                "Location",
                "L",
                "World-space text location.",
                GH_ParamAccess.list);

            pManager.AddColourParameter(
                "Colour",
                "C",
                "Text colour.",
                GH_ParamAccess.item,
                Color.Black);

            pManager.AddGenericParameter(
                "Annotation Setting",
                "S",
                "The annotation setting",
                GH_ParamAccess.item
            );
            pManager[3].Optional = true;

            pManager.AddNumberParameter(
                "Pointer_t",
                "t",
                "Time parameter controlling text opacity from 0 to 1.",
                GH_ParamAccess.list,
                1.0);
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var texts = new List<string>();
            var locations = new List<Point3d>();
            var times = new List<double>();

            if (!DA.GetDataList(0, texts) ||
                !DA.GetDataList(1, locations))
            {
                ClearDisplayContents();
                return;
            }

            var colour = Color.Black;
            var annotationsetting = new AnnotationDisplaySetting();

            DA.GetData(2, ref colour);
            DA.GetData(3, ref annotationsetting);
            DA.GetDataList(4, times);

            if (times.Count == 0)
                times.Add(1.0);

            var count = Math.Max(texts.Count, locations.Count);
            var contents = new List<DisplayAnnotationContent>();

            for (var index = 0; index < count; index++)
            {
                if (texts.Count == 0 || locations.Count == 0)
                    break;

                var text = texts[Math.Min(index, texts.Count - 1)];
                var location =
                    locations[Math.Min(index, locations.Count - 1)];

                var content =
                    new DisplayAnnotationContent(text, location, colour, annotationsetting);

                content.SetT(times[Math.Min(index, times.Count - 1)]);

                if (content.IsValid)
                    contents.Add(content);
            }

            _pipeline.Stage = SelectedRenderStage;
            _pipeline.SetContents(contents);
            SynchronizePreviewState();
        }

        public override BoundingBox ClippingBox =>
            _pipeline?.ClippingBox ?? BoundingBox.Empty;
    }
}

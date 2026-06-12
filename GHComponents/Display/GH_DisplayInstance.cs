using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Need debugs
    /// </summary>
    public sealed class GH_DisplayInstance : GH_DisplayGeometryAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        private readonly RenderInstancePipeline _pipeline;
        protected override IRenderPipeline renderPipeline => _pipeline;

        public GH_DisplayInstance()
            : base(
                "Display Instance",
                "DispInstance",
                "Display Rhino block instance references through a custom render pipeline.")
        {
            _pipeline = new RenderInstancePipeline(
                new List<DisplayInstanceContent>(), this);

            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_pipeline);
            _conduit.Enabled = true;
        }

        public override Guid ComponentGuid =>
            new Guid("5f1e9a9c-6c77-4c7f-b2da-7e12d7f9a0e1");

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Instance",
                "I",
                "Rhino block instance reference geometry.",
                GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(
            GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var values = new List<IGH_Goo>();
            if (!DA.GetDataList(0, values))
            {
                ClearDisplayContents();
                SynchronizePreviewState();
                return;
            }

            var document = Rhino.RhinoDoc.ActiveDoc;
            var contents = new List<DisplayInstanceContent>();

            foreach (var value in values)
            {
                var instance =
                    value?.ScriptVariable() as InstanceReferenceGeometry;

                if (instance == null)
                    continue;

                var content =
                    new DisplayInstanceContent(instance, document);

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

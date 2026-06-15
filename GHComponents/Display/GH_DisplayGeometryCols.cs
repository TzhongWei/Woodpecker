using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayGeometryCols : GH_DisplayGeometryAbstract
    {
        public override Guid ComponentGuid => new Guid("71dd5924-1123-4fca-9ccb-4eabe72b4e44");
        public GH_DisplayGeometryCols() : base("Display Geometry Cols", "DispGeoCols", "Display geometry with custom colors in the viewport. Based on the pointer_t, the colors will be interpolated.")
        {
            _renderGeometryPipeline = new RenderGeometryPipeline(
                new List<DisplayGeometryContentCols>(),
                this,
                GeometryRenderMode.Shaded
                );
            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_renderGeometryPipeline);
            _conduit.Enabled = true;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Geom_Cols;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Linear", "L", "Whether to use linear interpolation for colors. If false, it will use exponential interpolation.", GH_ParamAccess.item, true);
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to display.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Colors", "Cols", "Colors for the geometry. The structure should match the Geometry input.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Width", "W", "Width of the geometry display.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for color interpolation (0 to 1).", GH_ParamAccess.tree, 1.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            /// No output
        }
        protected override IRenderPipeline renderPipeline { get { return _renderGeometryPipeline; } }
        private readonly RenderGeometryPipeline _renderGeometryPipeline;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree<IGH_GeometricGoo>("Geometry", out var geometryTree) ||
                geometryTree == null ||
                geometryTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }

            var linear = false;
            DA.GetData("Linear", ref linear);

            if (!DA.GetDataTree<GH_Colour>("Colors", out var colTree) ||
                colTree == null ||
                colTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }

            var width = 1;
            DA.GetData("Width", ref width);

            if (!DA.GetDataTree<GH_Number>("Pointer_t", out var tTree) ||
                tTree == null ||
                tTree.DataCount == 0)
            {
                ClearDisplayContents();
                return;
            }

            var iColsList = new List<List<Color>>();
            var iGeomsList = new List<List<GeometryBase>>();
            var tList = new List<double>();
            for (int i = 0; i < geometryTree.Branches.Count; i++)
            {
                var geometries = geometryTree.Branches[i]
                    .Select(x => x?.ScriptVariable() as GeometryBase)
                    .ToList();

                var colSeleted = Math.Min(colTree.Branches.Count - 1, i);
                var colours = colTree[colSeleted]
                    .Where(x => x != null)
                    .Select(x => x.Value)
                    .ToList();

                var tSelected = Math.Min(tTree.Branches.Count - 1, i);
                var timeBranch = tTree.Branches[tSelected];

                if (geometries.Count == 0 ||
                    colours.Count == 0 ||
                    timeBranch.Count == 0)
                    continue;

                iGeomsList.Add(geometries);
                iColsList.Add(colours);
                tList.Add(timeBranch[0]?.Value ?? 1.0);
            }

            var contents = new List<DisplayGeometryContentCols>();
            for (int i = 0; i < iGeomsList.Count; i++)
            {
                for (int j = 0; j < iGeomsList[i].Count; j++)
                {
                    if (iGeomsList[i][j] == null || !iGeomsList[i][j].IsValid) continue;

                    var content = new DisplayGeometryContentCols(
                        iGeomsList[i][j],
                        iColsList[i],
                        linear)
                    {
                        Width = width
                    };

                    content.SetT(tList[i]);
                    contents.Add(content);
                }
            }
            _renderGeometryPipeline.Stage = SelectedRenderStage;
            _renderGeometryPipeline.SetContents(contents);
            SynchronizePreviewState();
        }
        // rgba(255, 81, 81, 0.7) rgba(220, 255, 81, 0.7) rgba(81, 101, 255, 0.7) rgba(203, 52, 249, 0.7)}
        protected override List<Color> optionColours { get; set; } = new List<Color>
        {
            Color.FromArgb(70, 255, 81, 81),
            Color.FromArgb(70, 220, 255, 81),
            Color.FromArgb(70, 81, 101, 255),
            Color.FromArgb(70, 203, 52, 249)
        };

        protected override RenderStage SelectedRenderStage
        {
            get
            {
                switch (state)
                {
                    case 0: return RenderStage.PreDrawObjects;
                    case 1: return RenderStage.Foreground;
                    case 2: return RenderStage.PostDrawObjects;
                    default: return RenderStage.Grasshopper;
                }
            }
        }
        public override void Switcher()
        {
            state = (state + 1) % 4;
            (Attributes as ButtonUIAttributesState)?.UpdateSelectedIndex(state);
            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
            ExpireSolution(true);
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesState(this, new List<string>{
                "PreDraw",
                "Foreground",
                "PostDraw",
                "Grasshopper"
            }, Switcher, optionColours, initialstate: state
           );
            (m_attributes as ButtonUIAttributesState)?.UpdateSelectedIndex(state);
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (SelectedRenderStage == RenderStage.Grasshopper)
                _renderGeometryPipeline.Render(args.Display);
        }
        public override BoundingBox ClippingBox =>
            _renderGeometryPipeline?.ClippingBox ?? BoundingBox.Empty;
    }
}

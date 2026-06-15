using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using System.Windows.Forms;
using Rhino.Display;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DisplayGeometry: GH_DisplayGeometryAbstract
    {
        public override Guid ComponentGuid => new Guid("098db1b9-d691-444e-a8ea-4063d55515b4");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to display.", GH_ParamAccess.tree);
            pManager.AddColourParameter("Color", "Col", "Color of the geometry.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
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

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No outputs needed for display component
        }

        protected override IRenderPipeline renderPipeline {get {return _renderGeometryPipeline;}}
        private readonly RenderGeometryPipeline _renderGeometryPipeline;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Geom;
        public GH_DisplayGeometry()
            : base(
                "Display Geometry",
                "DispGeo",
                "Display geometry in the viewport with custom colour and opacity.")
        {
            _renderGeometryPipeline = new RenderGeometryPipeline(
                new List<DisplayGeometryContent>(),
                this,
                GeometryRenderMode.Shaded
                );

            _conduit = new DisplayGeometryConduit();
            _conduit.Register(_renderGeometryPipeline);
            _conduit.Enabled = true;
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree<IGH_GeometricGoo>(0, out var geometryTree) ||
                geometryTree == null)
            {
                ClearDisplayContents();
                return;
            }

            var colour = Color.Black;
            DA.GetData(1, ref colour);

            GH_Structure<GH_Number> timeTree = null;
            DA.GetDataTree(2, out timeTree);

            var contents = new List<DisplayGeometryContent>();

            for (var branchIndex = 0;
                 branchIndex < geometryTree.Branches.Count;
                 branchIndex++)
            {
                var geometryBranch = geometryTree.Branches[branchIndex];
                var timeBranch = GetTimeBranch(timeTree, branchIndex);

                for (var itemIndex = 0;
                     itemIndex < geometryBranch.Count;
                     itemIndex++)
                {
                    var geometry =
                        geometryBranch[itemIndex]?.ScriptVariable()
                        as GeometryBase;

                    if (geometry == null || !geometry.IsValid)
                        continue;

                    var content =
                        new DisplayGeometryContent(geometry, colour);

                    content.SetT(GetTimeValue(timeBranch, itemIndex));
                    contents.Add(content);
                }
            }

            _renderGeometryPipeline.Stage = SelectedRenderStage;
            _renderGeometryPipeline.SetContents(contents);
            SynchronizePreviewState();
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (SelectedRenderStage == RenderStage.Grasshopper)
                _renderGeometryPipeline.Render(args.Display);
        }

        public override BoundingBox ClippingBox =>
            _renderGeometryPipeline?.ClippingBox ?? BoundingBox.Empty;


        private static IList<GH_Number> GetTimeBranch(
            GH_Structure<GH_Number> tree,
            int branchIndex)
        {
            if (tree == null || tree.Branches.Count == 0)
                return null;

            var index = Math.Min(branchIndex, tree.Branches.Count - 1);
            return tree.Branches[index];
        }

        private static double GetTimeValue(
            IList<GH_Number> branch,
            int itemIndex)
        {
            if (branch == null || branch.Count == 0)
                return 1.0;

            var index = Math.Min(itemIndex, branch.Count - 1);
            return branch[index]?.Value ?? 1.0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    [Obsolete]
    /// <summary>
    /// Displays multiple vectors with shared or per-branch display settings. Input points and vectors become viewport arrows for checking direction and magnitude. Inputs include ArrowTarget, ArrowDirection, VectorDisplaySetting, and Pointer_t.
    /// </summary>
    public class GH_VectorsDisplay : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_VectorsDisplay():base("Vectors Display", "VsD", "Display a tree of vectors as viewport arrows with optional styling and fade timing.", "Woodpecker", "Display"){}
        public override Guid ComponentGuid => new Guid("b5f916b8-b5c6-4da1-bad6-25aa6544a1fa");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Display_Vec;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("ArrowTarget", "AT", "Arrow start or target points.", GH_ParamAccess.tree);
            pManager.AddVectorParameter("ArrowDirection", "AV", "Arrow direction vectors.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("VectorDisplaySetting", "VDs", "Optional vector display style settings.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.tree, 1.0);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        private DataTree<Point3d> _arrowTarget;
        private DataTree<Vector3d> _arrowDirection;
        private VectorDisplaySetting _displaySetting;
        private DataTree<double> _pointer_ts;
        protected override void BeforeSolveInstance()
        {
            _body = new List<LineCurve>();
            _pointer_ts = new DataTree<double>();
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _arrowDirection = new DataTree<Vector3d>();
            _arrowTarget = new DataTree<Point3d>();
            _pointer_ts = new DataTree<double>();
            //var arrowDirection = new Vector3d();
            var VDs = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};

            DA.GetDataTree<GH_Point>("ArrowTarget", out var arrowTarget);
            DA.GetDataTree<GH_Vector>("ArrowDirection", out var arrowDirection);
            var okSetting = DA.GetData("VectorDisplaySetting", ref VDs);
            DA.GetDataTree<GH_Number>("Pointer_t", out var ts);
            
            DataUtil.GH_Structure2GH_DataTree(arrowTarget, ref _arrowTarget);
            DataUtil.GH_Structure2GH_DataTree(ts, ref _pointer_ts);
            DataUtil.GH_Structure2GH_DataTree(arrowDirection, ref _arrowDirection);

            // for(int i = 0; i < arrowTarget.Branches.Count; i++)
            // {
            //     _arrowTarget.AddRange(arrowTarget.Branches[i].Select(x => x.Value));
            // }
            // for(int i = 0; i < ts.Branches.Count; i++)
            // {
            //     _pointer_ts.AddRange(ts.Branches[i].Select(x => x.Value));
            // }
            // for(int i = 0; i < arrowDirection.Branches.Count; i++)
            // {
            //     _arrowDirection.AddRange(arrowDirection.Branches[i].Select( x=> x.Value));
            // }
            

            _pointer_ts = DataUtil.AlignDataTree(_arrowTarget, _pointer_ts);
            _arrowDirection = DataUtil.AlignDataTree(_arrowTarget, _arrowDirection);

            var tempBody = new List<LineCurve>();
            for(int i = 0; i < _arrowTarget.AllData().Count; i++)
            {
                var tempLine = new LineCurve(_arrowTarget.AllData()[i], _arrowTarget.AllData()[i] + _arrowDirection.AllData()[i] * VDs.Length);
                tempBody.Add(tempLine);
            }
            if(tempBody.SequenceEqual(_body))
            {
                this.ExpireSolution(true);
            }
            _body = tempBody;
            if (!okSetting)
                VDs = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};
            
            _displaySetting = VDs;
        }
        public override BoundingBox ClippingBox
        {
            get
            {
                if (_body == null || _body.Count == 0)
                    return BoundingBox.Empty;

                BoundingBox box = BoundingBox.Empty;

                if (_body != null)
                    box.Union(_body.Aggregate(new BoundingBox(), (acc, t) => {acc.Union(t.GetBoundingBox(true)); return acc; }));

                return box;
            }
        }
        private List<LineCurve> _body;

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if(_arrowTarget == null) return;
            for(int i = 0; i < _pointer_ts.BranchCount; i++)
            {
                for(int j = 0; j < _pointer_ts.Branch(i).Count; j++)
                {
                    if(_pointer_ts.Branch(i)[j] > 1e-2)
                    {
                        VectorDisplay.DrawLinearArrow(this, args, _arrowTarget.Branch(i)[j], _arrowDirection.Branch(i)[j], _displaySetting, _pointer_ts.Branch(i)[j]);
                    }
                }
            }
        }
    }
}

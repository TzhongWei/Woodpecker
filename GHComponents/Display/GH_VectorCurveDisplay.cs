
using System;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_VectorCurveDisplay: GH_Component
    {
        public GH_VectorCurveDisplay():base("Vector Curve Display", "VCD", "Display vector on a curve with a curved arrow", "Woodpecker", "Display")
        {
            
        }

        public override Guid ComponentGuid => new Guid("f56c21d6-194e-43d8-ab91-75e98322162f");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curves on which curved arrows are displayed.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Curve_t", "Ct", "Normalized curve parameter used to place or grow the vector display.", GH_ParamAccess.item);
            pManager.AddGenericParameter("VectorDisplaySetting", "VDs", "Optional vector display style settings.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.item, 1.0);
            pManager[2].Optional = true;
        }
        private DataTree<Curve> _crvTree;
        private VectorDisplaySetting _vectorDisplaySetting;
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        public override BoundingBox ClippingBox
        {
            get
            {
                if(_crvTree == null || _crvTree.AllData().Count == 0)
                return BoundingBox.Empty;

                else
                {
                    return _crvTree.AllData().Aggregate(new BoundingBox(), (acc, g) => {acc.Union(g.GetBoundingBox(true)); return acc; });
                }
            }
        }
        private double _crv_t;
        private double _pointer_t;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree<GH_Curve>(0, out var crvTree);
            var crv_t = 0.0;
            var pointer_t = 1.0;
            var VDs = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};
            DA.GetData("Curve_t", ref crv_t);
            var okSetting = DA.GetData("VectorDisplaySetting", ref VDs);
            DA.GetData("Pointer_t", ref pointer_t);
            this._crvTree = new DataTree<Curve>();
            DataUtil.GH_Structure2GH_DataTree(crvTree, ref this._crvTree);
            // for(int i = 0; i < crvTree.Branches.Count; i++)
            // {
            //     this._crvTree.AddRange(crvTree.Branches[i].Select(x => x.Value));
            // }


            if(!okSetting)
            {
                VDs = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};
            }
            _crv_t = crv_t;
            _pointer_t = pointer_t;
        }
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if(_pointer_t <= 1e-2) return; //Draw nothing
            if(_crvTree == null || _crvTree.AllData().Count == 0) return;
            for(int i = 0; i < _crvTree.AllData().Count; i++)
            {
                VectorDisplay.DrawCurveArrow(this, args, _crvTree.AllData()[i], _crv_t, this._vectorDisplaySetting, _pointer_t);    
            }
        }
    }
}

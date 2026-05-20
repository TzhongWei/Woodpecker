using Eto.Forms;
using GH_IO.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    [Obsolete]
    /// <summary>
    /// Displays vectors at points with configurable colour, scale, and arrow style. Point and vector inputs define the preview arrows shown in the viewport. Inputs include ArrowTarget, ArrowDirection, VectorDisplaySetting, and Pointer_t.
    /// </summary>
    public class GH_VectorDisplay : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_VectorDisplay() : base("Vector Display", "VD", "Legacy viewport vector-arrow display component.", "Woodpecker", "Display") { }

        public override Guid ComponentGuid => new Guid("32bd1fc4-14ae-4bf3-b845-0944e5e63ae2");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("ArrowTarget", "AT", "Arrow start or target points.", GH_ParamAccess.tree);
            pManager.AddVectorParameter("ArrowDirection", "AV", "Arrow direction vectors.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("VectorDisplaySetting", "VDs", "Optional vector display style settings.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pointer_t", "t", "Time parameter for fade effect (0 to 1).", GH_ParamAccess.item, 1.0);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        private DataTree<Point3d> _arrowTarget;
        private DataTree<Vector3d> _arrowDirection;
        private VectorDisplaySetting _displaySetting;
        private double _pointer_t = 1.0;
        protected override void BeforeSolveInstance()
        {
            _body = new List<LineCurve>();
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //var arrowDirection = new Vector3d();
            var VDs = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};
            var t = 0.0;
            DA.GetDataTree<GH_Point>("ArrowTarget", out var arrowTarget);
            //DA.GetData("ArrowDirection", ref arrowDirection);
            DA.GetDataTree<GH_Vector>("ArrowDirection", out var arrowDirection);
            var okSetting = DA.GetData("VectorDisplaySetting", ref VDs);
            DA.GetData("Pointer_t", ref t);
            _arrowTarget = new DataTree<Point3d>();
            _arrowDirection = new DataTree<Vector3d>();

            DataUtil.GH_Structure2GH_DataTree(arrowTarget, ref _arrowTarget);
            DataUtil.GH_Structure2GH_DataTree(arrowDirection, ref _arrowDirection);
            // for(int i = 0; i < arrowTarget.Branches.Count; i++)
            // {
            //     _arrowTarget.AddRange(arrowTarget.Branches[i].Select(x => x.Value));
            // }
            // for(int i = 0; i < arrowDirection.Branches.Count; i++)
            // {
            //     _arrowDirection.AddRange(arrowDirection.Branches[i].Select(x => x.Value));
            // }
            _arrowDirection = DataUtil.AlignDataTree(_arrowTarget, _arrowDirection);
            
            
            if (!okSetting)
                VDs = new VectorDisplaySetting(){Length = 10, ArrowheadSize = 30, ArrowRelativeSize = 0};
            _pointer_t = t;
            _displaySetting = VDs;
            var tempList = new List<LineCurve>();
            for(int i = 0; i < _arrowTarget.AllData().Count; i++)
            {
                var tempLine = new LineCurve(_arrowTarget.AllData()[i], _arrowTarget.AllData()[i] + _arrowDirection.AllData()[i] * VDs.Length);
                tempList.Add(tempLine);
            }
            if(tempList.SequenceEqual(_body))
            {
                this.ExpireSolution(false);
            }
            _body = tempList;
            
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
            if(_pointer_t <= 1e-2) return; //Draw nothing
            if(_arrowTarget == null) return;
            for(int i = 0 ; i < _arrowTarget.AllData().Count; i++)
                VectorDisplay.DrawLinearArrow(this, args, _arrowTarget.AllData()[i], _arrowDirection.AllData()[i], _displaySetting, _pointer_t);
        }
    }
}

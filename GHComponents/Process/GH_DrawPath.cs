using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Processing;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_DrawPath : GH_Component
    {
            public GH_DrawPath():base(
            "Draw Pathes",
            "DP",
            "Generate progressive path segments along one or multiple curves based on a normalized parameter t. Supports directional growth control, per-segment speed, and tool plane generation along the path.",
            "Woodpecker",
            "Process")
        {
            
        }

        public override Guid ComponentGuid => new Guid("eb44c30d-9b01-403c-9792-3907f17f7185");
        public bool _tryfollowTangent = true;
        protected void ToogleTryFollowTangent(object sender, EventArgs e)
        {
            _tryfollowTangent = !_tryfollowTangent;
            ExpireSolution(true);
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Plane try follow tangent", ToogleTryFollowTangent, true, _tryfollowTangent);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            
            pManager.AddCurveParameter("Paths", "P", "Input curves defining the path sequence.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Pointer_t", "t", "Normalized parameter (0–1) controlling the progression along the path.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Tool_Normal", "Dir", "Reference direction for constructing the tool plane.", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager.AddNumberParameter("Distance", "Dist", "Offset distance along the normal direction for positioning the tool plane.", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("ChangeEnd", "CE", "Controls the direction of curve growth. If true, once t reaches 1, the path will start progressing from the beginning of the first curve. If false, the path continues extending from the end of the last curve.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Speed", "S", "Per-curve speed factors controlling relative progression across multiple paths.", GH_ParamAccess.list, new List<double>{1.0});
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("FinishedPaths", "FPs", "Resulting partial or completed path segments at the given parameter t.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("ToolPlane", "TP", "Tool plane positioned at the current evaluation point along the path.", GH_ParamAccess.item);
        }
        private PathUtil _pathUtil = null;
        private List<Curve> _crv = null;
        private bool _changeEnd = false;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cpath = new List<Curve>();
            var _t = 0.0;
            var dir = Vector3d.ZAxis;
            var dist = 0.0;
            var ch = false;
            List<double> speed = new List<double>();

            DA.GetDataList("Paths", cpath);
            DA.GetData("Pointer_t", ref _t);
            DA.GetData("Tool_Normal", ref dir);
            DA.GetData("Distance", ref dist);
            DA.GetData("ChangeEnd", ref ch);
            DA.GetDataList("Speed", speed);

            if(_crv == null || !GeometryUtil.CompareCrv(_crv, cpath) || _changeEnd != ch)
            {
                _pathUtil = new PathUtil(cpath, ch);
                _crv = cpath;
                _changeEnd = ch;
            }
            _pathUtil.Compute(_t, dir, dist, speed, out var _pl, out var _finishPath, _tryfollowTangent);

            DA.SetDataList("FinishedPaths", _finishPath);
            DA.SetData("ToolPlane", _pl);
        }
        public override bool Write(GH_IWriter writer)
        {
            var result = base.Write(writer);
            writer.SetBoolean("followTangent", _tryfollowTangent);
            return result;
        }
        public override bool Read(GH_IReader reader)
        {
            var result = base.Read(reader);
            if(result &= reader.TryGetBoolean("followTangent", ref _tryfollowTangent))
            {
                ExpireSolution(true);
            }
            return result;
        }
    }
}
using Grasshopper;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_SegmentiseTimeslotNonlinear : GH_TimelineAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public GH_SegmentiseTimeslotNonlinear() : base("Segmentise Timeslot in Nonlinear", "STNL", "Segmentise timeslot nonlinearly") { }

        public override System.Guid ComponentGuid => new System.Guid("b1c2d3e4-f5a6-7890-1234-56789abcdef0");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("pointer_t", "t", "Pointer time to segmentise around, t = [0,1]", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Segments", "S", "Number of segments to create", GH_ParamAccess.item);
            pManager.AddNumberParameter("Overlap", "O", "Overlap between segments, Overlap = [0,1)", GH_ParamAccess.list, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("segmented_Pointer_t", "ts", "Segmented pointer time t list", GH_ParamAccess.list);
            pManager.AddNumberParameter("Segment_Length", "L", "Length of each segment", GH_ParamAccess.item);
        }

        protected override string ShowTimeSetupDescription()
        {

            var message = $"Pointer t: {_pointer_t}\n Segments: [{string.Join(", ", _segmented_Pointer_t)}]";
            return message;
        }
        private List<double> _segmented_Pointer_t = new List<double>();
        private double _pointer_t = 0.0;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pointer_t = 0.0;
            var segments = 0;
            var overlaps = new List<double>();
            DA.GetData("pointer_t", ref pointer_t);
            DA.GetData("Segments", ref segments);
            DA.GetDataList("Overlap", overlaps);
            var ts = new List<double>();
            var len = 0.0;
            TimelineSetting.SegmentiseTimeslotNonLinear(pointer_t, segments, overlaps, ref ts, ref len);
            this._segmented_Pointer_t = ts;
            this._pointer_t = pointer_t;

            this.MessageSetup();

            DA.SetDataList("segmented_Pointer_t", ts);
            DA.SetData("Segment_Length", len);
        }
    }
}
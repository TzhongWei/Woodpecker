using System.Collections.Generic;
using System;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_SegmentiseTimeslotLinear : GH_TimelineAbstract
    {
        public GH_SegmentiseTimeslotLinear() : base("Segmentise Timeslot in Linear", "STL", "Segmentise timeslot linearly") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("a1b2c3d4-e5f6-7890-1234-56789abcdef0");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Pointer_t", "t", "Pointer time to segmentise around, t = [0,1]", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Segments", "S", "Number of segments to create", GH_ParamAccess.item);
            pManager.AddNumberParameter("Overlap", "O", "Overlap between segments, Overlap = [0,1)", GH_ParamAccess.item, 0.0);
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
            var overlap = 0.0;
            DA.GetData("Pointer_t", ref pointer_t);
            DA.GetData("Segments", ref segments);
            DA.GetData("Overlap", ref overlap);
            var ts = new List<double>();
            var len = 0.0;
            TimelineSetting.SegmentiseTimeslotLinear(pointer_t, segments, overlap, ref ts, ref len);

            this._segmented_Pointer_t = ts;
            this._pointer_t = pointer_t;

            this.MessageSetup();

            DA.SetDataList("segmented_Pointer_t", ts);
            DA.SetData("Segment_Length", len);
        }
    }
}
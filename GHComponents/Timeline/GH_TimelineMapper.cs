using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    internal class GH_TimelineMapper : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_TimelineMapper() : base("TimelineMapper", "TLMapper", "Interactively edit and output a timeline interval.", "Woodpecker", "Timeline")
        {

        }

        public override Guid ComponentGuid => new Guid("6609a593-0967-406c-aa73-abee171b83c8");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Optional input timeline used to initialise the mapper range.", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Timeline", "TL", "Timeline interval currently defined by the mapper.", GH_ParamAccess.item);
        }
        private Interval _timeline = new Interval(0, 1);
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var Temptimeline = new Interval();
            DA.GetData("Timeline", ref Temptimeline);

            if (Temptimeline != _timeline)
            {
                _fromValue = Temptimeline.Min;
                _toValue = Temptimeline.Max;

                this.Attributes?.ExpireLayout();

                ((TimelineUIAttributes)this.Attributes).SetFromValue(_fromValue);
                ((TimelineUIAttributes)this.Attributes).SetToValue(_toValue);

                this.OnDisplayExpired(true);
            }
            ((TimelineUIAttributes)this.Attributes).IsEditMode = IsEditMode;
            _timeline = Temptimeline;
            DA.SetData("Timeline", _timeline);
        }
        public bool IsEditMode
        {
            get
            {
                return Params.Input.Count == 0 || Params.Input[0].SourceCount == 0;
            }
        }
        public override void CreateAttributes()
        {

            m_attributes = new TimelineUIAttributes(this, OnChangeFrom, OnChangeTo, 0, 0);
        }
        private double _fromValue = 0;
        private double _toValue = 0;
        public void OnChangeFrom(double Fromvalue)
        {
            this._fromValue = Fromvalue;
        }
        public void OnChangeTo(double Tovalue)
        {
            this._toValue = Tovalue;
        }
    }
}

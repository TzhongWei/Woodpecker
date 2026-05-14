using System;
using Rhino.Geometry;
using Rhino.UI.Theme;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.Geometry.Processing
{
    public abstract class GeometryActionAbstract
    {
        public string Name;
        public string Message { get; protected set; }
        public string Description;
        private readonly Interval _originalTimeline; // The original timeline can be different interval ranges
        public Interval OriginalTimeline => _originalTimeline;
        private Interval _timeline;  // All timeline is [0,1], which is normalised
        public Interval Timeline => _timeline.IsValid ? _timeline : (_timeline = new Interval(0, 1));
        public bool IsActive(double t) => Timeline.IncludesParameter(t);
        public bool HasStarted(double t) => t >= Timeline.Min;
        public bool IsFinish = false;
        public GeometryActionAbstract(string actionName, double startT = 0, double endT = 1)
        {
            this.Name = actionName;
            this._originalTimeline = new Interval(startT, endT);
        }
        internal void ReclampTimeline(Interval Newtimeline)
        {
            _timeline = Newtimeline;
        }
        public virtual void Initialised()
        {
            _timeline = Interval.Unset;
            this.IsFinish = false;
        }
        public GeometryActionAbstract(string actionName, Interval timeline)
        {
            this.Name = actionName;
            this._originalTimeline = timeline;
        }
        [Obsolete]
        public virtual bool PreEvaluate()
        {
            if (!Timeline.IsValid)
            {
                return false;
            }
            return true;
        }
        public virtual bool PreEvaluate(GeometryContent content)
        {
            if (!Timeline.IsValid)
            {
                return false;
            }
            return true;
        }
        public abstract bool TryApply(GeometryContent content, double currentT);

        [Obsolete("Use TryApply(GeometryContent content, double currentT) instead.")]
        public abstract bool TryApply_OLD(GeometryBase input, double currentT, out GeometryBase output);
        public virtual bool PostEvaluate(GeometryContent content)
        {
            return true;
        }
        [Obsolete]
        public virtual bool PostEvaluate()
        {
            return true;
        }
        protected double Normalize(double t)
        {
            if (Timeline.Length <= 0) return 0;
            return Math.Max(0, Math.Min(1, (t - Timeline.Min) / Timeline.Length));
        }
    }
    public abstract class TransformTSAction : GeometryActionAbstract
    {
        protected TransformTSAction(string TransformActionName, double StartT = 0, double EndT = 1) : base(TransformActionName, StartT, EndT) { }
        public abstract Transform GetTransform(GeometryContent content, double NormalT);
        [Obsolete]
        public virtual Transform GetTransform(double NormalT) => Transform.Identity;
        public override bool TryApply(GeometryContent content, double currentT)
        {
            Message = string.Empty;

            if (content == null)
                return false;

            var nt = Normalize(currentT);
            if (!HasStarted(currentT))
                return true;

            var isFinished = currentT >= Timeline.Max;
            if (isFinished)
                nt = 1.0;

            Message = $"{Name} active | t = {nt:F2}";
            return content.NewTransformation(GetTransform(content, nt), Message, isFinished);
        }

        [Obsolete("Use TryApply(GeometryContent content, double currentT) instead.")]
        public override bool TryApply_OLD(GeometryBase input, double currentT, out GeometryBase output)
        {
            
            if (IsFinish)
            {
                output = input.Duplicate();
                return true;
            }

            output = input.Duplicate();

            var nt = Normalize(currentT);

            // Test if t is in the timeline, or return;
            if (!HasStarted(currentT))
                return true;

            output = input.Duplicate();
            output.Transform(GetTransform(nt));

            Message = $"{Name} active | t = {nt:F2}";

            return true;
        }
    }
}

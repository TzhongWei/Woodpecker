
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryAnimation
    {
        private GeometryBase _source;
        private GeometryBase _current;
        private List<GeometryActionAbstract> _actions;
        public GeometryAnimation(GeometryBase geomobject)
        {
            _source = geomobject.Duplicate();
            _current = _source.Duplicate();
            _actions = new List<GeometryActionAbstract>();
        }
        internal void Initialised()
        { 
            _current = _source.Duplicate();
            foreach(var action in _actions)
            {
                action.Initialised();
            }
        }
        // Normalised all timeline in the list of geometryaction
        internal void RemappingTimeline()
        {
            double min = 0, max = 1;
            TimelineSetting.IntervalRange(_actions.Select(x => x.OriginalTimeline).ToList(), ref min, ref max);
            // remapping all timeline within 0 and 1
            for(int i = 0; i < _actions.Count; i++)
            {
                var range = max-min;
                if(range <= 1e-9) range = 1;
                var newTimeline = new Interval(
                    (_actions[i].OriginalTimeline.Min - min) / range
                    ,
                    (_actions[i].OriginalTimeline.Max - min) / range
                    );
                _actions[i].ReclampTimeline(newTimeline);
            }
        }
        internal GeometryBase GetGeometry() => _current.Duplicate();
        public void AddAction(GeometryActionAbstract action)
        {
            _actions.Add(action);
        }
        public void AddRangeAction(IEnumerable<GeometryActionAbstract> actions)
        {
            _actions.AddRange(actions);
        }
        public string TotalActionDomain
        {
            get
            {
                double min = 0, max = 0;
                TimelineSetting.IntervalRange(
                _actions.Select(x => x.Timeline).ToList(), ref min, ref max
            );
                return TimelineSetting.TimelineDescription(new Interval(min, max));
            }
        }
        public List<string> Message {get; private set;} = new List<string>();
        public virtual bool PreEvaluate(GeometryContent content, double t)
        {
            if (content == null)
                return false;

            Message.Clear();
            content.Initialised();

            var result = true;
            foreach(var action in _actions.OrderBy(x => x.Timeline.Min))
            {
                result &= action.PreEvaluate(content);
            }
            return result;
        }

        [Obsolete("Use PreEvaluate(GeometryContent content, double t) instead.")]
        public virtual bool PreEvaluate_OLD(double t)
        {
            _current = _source.Duplicate();
            var result = true;
            foreach(var action in _actions.OrderBy(x => x.Timeline.Min))
            {         
                result &= action.PreEvaluate();
            }
            return result;
        }
        public virtual bool Evaluate(GeometryContent content, double t)
        {
            if (content == null)
                return false;

            var result = true;
            foreach(var action in _actions.OrderBy(a => a.Timeline.Min))
            {
                result &= action.TryApply(content, t);
                if (!string.IsNullOrWhiteSpace(action.Message))
                    Message.Add(action.Message);
            }
            return result;
        }

        [Obsolete("Use Evaluate(GeometryContent content, double t) instead.")]
        public virtual bool Evaluate_OLD(double t)
        {
            var result = true;
            foreach(var action in _actions.OrderBy(a => a.Timeline.Min))
            {
                result &= action.TryApply_OLD(this._current, t, out this._current);
                if (!string.IsNullOrWhiteSpace(action.Message))
                    Message.Add(action.Message);
            }
            return result;
        }
        public virtual bool PostEvaluate(GeometryContent content, double t)
        {
            if (content == null)
                return false;

            var result = true;
            foreach(var action in _actions)
            {
                if(t > action.Timeline.Max)
                    result &= action.PostEvaluate(content);

            }
            return result;
        }

        [Obsolete("Use PostEvaluate(GeometryContent content, double t) instead.")]
        public virtual bool PostEvaluate_OLD(double t)
        {
            var result = true;
            foreach(var action in _actions)
            {
                if(t > action.Timeline.Max)
                    result &= action.PostEvaluate();
                
            }
            return result;
        }

        [Obsolete("Use PreEvaluate/Evaluate/PostEvaluate with GeometryContent instead.")]
        public GeometryBase EvaluateGeometry_OLD(double t)
        {
            GeometryBase result = _source.Duplicate();

            foreach (var action in _actions.OrderBy(a => a.Timeline.Min))
            {

                if (action.TryApply_OLD(result, t, out result))
                {
                    Message.Add(action.Message);
                }
            }
            return result;
        }

        [Obsolete("Use EvaluateGeometry_OLD(double t) for the old geometry-returning path, or the GeometryContent pipeline for new code.")]
        public GeometryBase Evaluate_Old(double t) => EvaluateGeometry_OLD(t);
    }
}

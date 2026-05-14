
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
        public virtual bool PreEvaluate(double t)
        {
            _current = _source.Duplicate();
            var result = true;
            foreach(var action in _actions.OrderBy(x => x.Timeline.Min))
            {         
                result &= action.PreEvaluate();
            }
            return result;
        }
        public virtual bool Evaluate(double t)
        {
            var result = true;
            foreach(var action in _actions.OrderBy(a => a.Timeline.Min))
            {
                result &= action.TryApply(this._current, t, out this._current);   
                Message.Add(action.Message);
            }
            return true;
        }
        public virtual bool PostEvaluate(double t)
        {
            var result = true;
            foreach(var action in _actions)
            {
                if(t > action.Timeline.Max)
                    result &= action.PostEvaluate();
                
            }
            return result;
        }

        [Obsolete]
        public GeometryBase Evaluate_Old(double t)
        {
            GeometryBase result = _source.Duplicate();

            foreach (var action in _actions.OrderBy(a => a.Timeline.Min))
            {

                if (action.TryApply(result, t, out result))
                {
                    Message.Add(action.Message);
                }
            }
            return result;
        }
    }
}
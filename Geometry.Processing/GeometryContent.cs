using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryContent
    {
        public Dictionary<string, object> CustomSetting { get; set; } = new Dictionary<string, object>();
        private readonly List<GeometryBase> _sourceGeoms = new List<GeometryBase>();
        private readonly List<GeometryBase> _currentGeoms = new List<GeometryBase>();
        public List<Transform> HistoryTransform { get; private set; } = new List<Transform>();
        public Stack<Transform> StackTransform { get; private set; } = new Stack<Transform>();
        private Transform _currentTS { get; set; }
        public List<string> Message = new List<string>();
        public GeometryAnimationPipeline geometryAnimationPipeline { get; private set; }
        public IReadOnlyList<GeometryBase> SourceGeoms => _sourceGeoms;
        public IReadOnlyList<GeometryBase> CurrentGeoms => _currentGeoms;
        internal void Initialised()
        {
            _currentGeoms.Clear();
            for (int i = 0; i < _sourceGeoms.Count; i++)
            {
                _currentGeoms.Add(_sourceGeoms[i].Duplicate());
            }
            _currentTS = Transform.Identity;
            HistoryTransform = new List<Transform>();
            HistoryTransform.Add(_currentTS);
            StackTransform = new Stack<Transform>();
            Message = new List<string>();
        }
        public Transform GetCurrentTransform() => _currentTS;
        public List<GeometryBase> GetCurrentGeometry() => _currentGeoms.Select(x => x.Duplicate()).ToList();

        public GeometryContent(GeometryAnimationPipeline geometryAnimationPipeline, GeometryBase Geom)
        {
            this.geometryAnimationPipeline = geometryAnimationPipeline;
            if (Geom != null)
                _sourceGeoms.Add(Geom.Duplicate());
            Initialised();
        }
        public GeometryContent(GeometryAnimationPipeline geometryAnimationPipeline, IEnumerable<GeometryBase> Geoms)
        {
            this.geometryAnimationPipeline = geometryAnimationPipeline;
            if (Geoms != null)
                _sourceGeoms.AddRange(Geoms.Where(x => x != null).Select(x => x.Duplicate()));
            Initialised();
        }
        public void AddGeometry(GeometryBase newGeom)
        {
            _sourceGeoms.Add(newGeom.Duplicate());
            var Last = _sourceGeoms.LastOrDefault().Duplicate();
            Last.Transform(_currentTS);
            _currentGeoms.Add(Last);
        }
        public bool RemoveGeometry(GeometryBase newGeom)
        => _sourceGeoms.Remove(newGeom);
        private void ApplyGeometry()
        {
            _currentGeoms.Clear();
            foreach (var sourceGeom in _sourceGeoms)
            {
                var duX = sourceGeom.Duplicate();
                duX.Transform(_currentTS);
                _currentGeoms.Add(duX);
            }
        }
        private void ApplyGeometry(Transform tempTS)
        {
            _currentGeoms.Clear();
            foreach (var sourceGeom in _sourceGeoms)
            {
                var duX = sourceGeom.Duplicate();
                duX.Transform(tempTS);
                _currentGeoms.Add(duX);
            }
        }
        public void Push(Transform transform) => this.StackTransform.Push(transform);
        public Transform Pop()
        {
            // #region 
            // # net48
            return this.StackTransform.Pop();
            // # net7.0-window and net7.0
            // this.StackTransform.TryPop(out var TS); 
            // return TS;
        }
        public bool NewTransformation(Transform transform, string Message, bool IsFinished = false)
        {
            this._currentTS = transform * _currentTS;
            if (IsFinished)
            {
                this.HistoryTransform.Add(this._currentTS);
                
                //ApplyGeometry();
            }
            ApplyGeometry(_currentTS);
            
            this.Message.Add(Message);
            return true;
        }

    }
}

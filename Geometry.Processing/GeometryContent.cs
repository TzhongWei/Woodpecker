using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Woodpecker.Animation.Geometry.Processing
{
    public class GeometryContent
    {
        public Dictionary<string, object> CustomSetting { get; set; } = new Dictionary<string, object>();
        public List<GeometryBase> SourceGeoms { get; private set; }
        public List<Transform> HistoryTransform { get; private set; } = new List<Transform>();
        public Stack<Transform> StackTransform { get; private set; } = new Stack<Transform>();
        public Transform CurrentTS { get; private set; }
        public List<GeometryBase> CurrentGeoms { get; private set; }
        public List<string> Message = new List<string>();
        public GeometryAnimationPipeline geometryAnimationPipeline { get; private set; }
        private void Initialised()
        {
            CurrentGeoms = new List<GeometryBase>(SourceGeoms);
            CurrentTS = Transform.Identity;
            HistoryTransform = new List<Transform>();
            HistoryTransform.Add(CurrentTS);
            StackTransform = new Stack<Transform>();
            Message = new List<string>();
        }
        public GeometryContent(GeometryAnimationPipeline geometryAnimationPipeline, GeometryBase Geom)
        {
            SourceGeoms = new List<GeometryBase>();
            SourceGeoms.Add(Geom.Duplicate());
            Initialised();
        }
        public GeometryContent(GeometryAnimationPipeline geometryAnimationPipeline, IEnumerable<GeometryBase> Geoms)
        {
            SourceGeoms = Geoms.Select(x => x.Duplicate()).ToList();
            Initialised();
        }
        public void AddGeometry(GeometryBase newGeom)
        {
            SourceGeoms.Add(newGeom.Duplicate());
            var Last = SourceGeoms.LastOrDefault().Duplicate();
            Last.Transform(CurrentTS);
            CurrentGeoms.Add(Last);
        }
        public bool RemoveGeometry(GeometryBase newGeom)
        => SourceGeoms.Remove(newGeom);
        private void ApplyGeometry()
        {
            CurrentGeoms = SourceGeoms.Select(x =>
                    {
                        var duX = x.Duplicate();
                        duX.Transform(CurrentTS);
                        return duX;
                    }).ToList();
        }
        private void ApplyGeometry(Transform tempTS)
        {
            CurrentGeoms = SourceGeoms.Select(x =>
            {
                var duX = x.Duplicate();
                duX.Transform(tempTS);
                return duX;
            }).ToList();
        }
        public void Push(Transform transform) => this.StackTransform.Push(transform);
        public Transform Pop(){
            // #region 
            // # net48
            return this.StackTransform.Pop();
            // # net7.0-window and net7.0
            // this.StackTransform.TryPop(out var TS); 
            // return TS;
            }
        public bool NewTransformation(Transform transform, string Message, bool IsFinished = false)
        {
            var tempCurrentTS = transform * CurrentTS;
            if (IsFinished)
            {
                this.HistoryTransform.Add(tempCurrentTS);
                this.CurrentTS = tempCurrentTS;
                ApplyGeometry();
            }
            else
            {
                ApplyGeometry(tempCurrentTS);
            }
            this.Message.Add(Message);
            return true;
        }

    }
}
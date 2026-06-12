using System.Collections;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.Geometry.Display
{
    public enum RenderStage
    {
        PreDrawObjects,
        PostDrawObjects,
        Foreground,
    }

    public interface IRenderPipeline
    {
        RenderStage Stage { get; set; }
        bool Enabled { get; set; }
        bool ShouldRender { get; }
        BoundingBox ClippingBox { get; }
        void Clear();
        void Render(DisplayPipeline display);
    }

    public abstract class RenderPipelineAbstract<TContent> : IRenderPipeline, IList<TContent>
        where TContent : IDisplayContent
    {
        protected readonly GH_Component m_component;
        public RenderStage Stage { get; set; } = RenderStage.PostDrawObjects;
        public bool Enabled { get; set; } = true;
        public bool ShouldRender
        {
            get
            {
                if (!Enabled || m_component == null || m_component.Hidden)
                    return false;

                var document = m_component.OnPingDocument();
                if (document == null ||
                    Instances.ActiveCanvas?.Document != document ||
                    document.PreviewMode == GH_PreviewMode.Disabled)
                    return false;

                return document.PreviewFilter != GH_PreviewFilter.Selected ||
                       (m_component.Attributes?.Selected ?? false);
            }
        }

        protected readonly List<TContent> m_Contents;

        public IReadOnlyList<TContent> Contents => m_Contents;

        public BoundingBox ClippingBox
        {
            get
            {
                var clippingBox = BoundingBox.Empty;

                foreach (var content in m_Contents)
                {
                    if (content == null || !content.Visible || !content.IsValid)
                        continue;

                    clippingBox.Union(content.ClippingBox);
                }

                return clippingBox;
            }
        }

        public int Count => m_Contents.Count;

        public bool IsReadOnly => false;

        public TContent this[int index]
        {
            get => m_Contents[index];
            set => m_Contents[index] = value;
        }

        public abstract void Render(DisplayPipeline display);

        protected RenderPipelineAbstract()
        {
            m_Contents = new List<TContent>();
            m_component = null;
        }

        protected RenderPipelineAbstract(IEnumerable<TContent> contents, GH_Component gH_Component) : this()
        {
            SetContents(contents);
            this.m_component = gH_Component;
        }

        public void Add(TContent content)
        {
            if (content != null)
                m_Contents.Add(content);   
        }

        public bool Remove(TContent content)
        {
            return content != null && m_Contents.Remove(content);
        }

        public void Clear()
        {
            m_Contents.Clear();
        }

        public void SetContents(IEnumerable<TContent> contents)
        {
            m_Contents.Clear();

            if (contents == null)
                return;

            foreach (var content in contents)
                Add(content);
        }

        public int IndexOf(TContent item)
        => m_Contents.IndexOf(item);

        public void Insert(int index, TContent item)
        {
            m_Contents.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_Contents.RemoveAt(index);
        }

        public bool Contains(TContent item)
            => m_Contents.Contains(item);
        

        public void CopyTo(TContent[] array, int arrayIndex)
        {
            m_Contents.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TContent> GetEnumerator()
        => m_Contents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        => m_Contents.GetEnumerator();
    }
}

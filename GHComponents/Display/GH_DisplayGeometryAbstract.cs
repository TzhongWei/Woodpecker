using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Geometry.Display;
using GH_IO.Serialization;
using Rhino.Display;

namespace Woodpecker.Animation.GHComponents
{
    public abstract class GH_DisplayGeometryAbstract : GH_Component
    {
        public GH_DisplayGeometryAbstract(string Name, string NickName, string Description) : base(Name, NickName, Description, "Woodpecker", "Display")
        {
        }
        protected int state = 0;
        protected virtual RenderStage SelectedRenderStage
        {
            get
            {
                switch (state)
                {
                    case 0: return RenderStage.PreDrawObjects;
                    case 1: return RenderStage.Foreground;
                    default: return RenderStage.PostDrawObjects;
                }
            }
        }

        protected virtual List<Color> optionColours {get; set;} = new List<Color> { Color.FromArgb(70, 255, 81, 81), Color.FromArgb(70, 220, 255, 81), Color.FromArgb(70, 81, 101, 255) }; // rgba(255, 81, 81, 0.7) rgba(220, 255, 81, 0.7) rgba(81, 101, 255, 0.7)

        public virtual void Switcher()
        {
            state = (state + 1) % 3;
            (this.Attributes as ButtonUIAttributesState).UpdateSelectedIndex(state);
            this.Attributes?.ExpireLayout();
            this.OnDisplayExpired(true);
            this.ExpireSolution(true);
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesState(this, new List<string>{
                "PreDraw",
                "Foreground",
                "PostDraw"
            }, Switcher, optionColours
            );
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("RenderStage", this.state);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetInt32("RenderStage", ref this.state);
            return base.Read(reader);
        }
        protected bool _objectChangedSubscribed;

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            if (!_objectChangedSubscribed)
            {
                ObjectChanged += ComponentObjectChanged;
                _objectChangedSubscribed = true;
            }

            SynchronizePreviewState();
        }
        protected abstract IRenderPipeline renderPipeline {get;}
        protected DisplayGeometryConduit _conduit;

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();
            renderPipeline?.Clear();
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            if (_objectChangedSubscribed)
            {
                ObjectChanged -= ComponentObjectChanged;
                _objectChangedSubscribed = false;
            }

            _conduit.Enabled = false;
            _conduit.Clear();
            Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
            base.RemovedFromDocument(document);
        }

        protected void ComponentObjectChanged(
            IGH_DocumentObject sender,
            GH_ObjectChangedEventArgs e)
        {
            if (e.Type == GH_ObjectEventType.Preview)
                SynchronizePreviewState();
        }

        protected void SynchronizePreviewState()
        {
            var previewEnabled = !Hidden;

            renderPipeline.Enabled = previewEnabled;
            _conduit.Enabled = previewEnabled;

            Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
        }

        protected void ClearDisplayContents()
        {
            renderPipeline?.Clear();
            SynchronizePreviewState();
        }

    }
}

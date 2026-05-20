using System.Collections.Generic;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Camera;
using System.Drawing;
using GH_IO.Serialization;
using System.Linq;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    /// <summary>
    /// Camera Motion Abstract component.
    /// </summary>
    public abstract class GH_CameraMotionAbstract : GH_Component, IHasMultipleActiveInstanceDocumentComponent
    {
        public string MultiTag => "CameraSetting";
        public bool HasMultipleActiveInstance()
        {
            var doc = this.OnPingDocument();
            if(doc == null) return false;
            var ActiveCameraComponent = doc.Objects.OfType<GH_CameraMotionAbstract>()
            .Where(x => x._isActive).ToList();

            if(ActiveCameraComponent.Count > 1)
                return true;
            else return false;
        }
        protected bool _isActive = false;
        public GH_CameraMotionAbstract(string Name, string NickName, string Description) : 
        base(Name, NickName, Description, "Woodpecker", "Camera")
        {
        }
        protected bool _applyCameraMotion = true;
        protected CameraParameter _cameraParam;

        private void ApplyCamera()
        {
            _applyCameraMotion = !_applyCameraMotion;
            var index = _applyCameraMotion ? 0 : 1;
             (this.m_attributes as ButtonUIAttributesState)?.UpdateSelectedIndex(index);
           
             this.ExpireSolution(false);
        }
        public override void CreateAttributes()
        {
            this.m_attributes = new ButtonUIAttributesState(this, new List<string>{"Apply Camera", "Camera Preview"}, ApplyCamera, 
            new List<Color>{Color.FromArgb(70, 255, 81, 81), Color.FromArgb(70, 220, 255, 81)},
            "Camera Motion to Viewport");
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("applyCameraMotion", _applyCameraMotion);

            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            if(reader.TryGetBoolean("applyCameraMotion", ref _applyCameraMotion))
            {
                return base.Read(reader);
            }
            return false;
        }
        protected override void AfterSolveInstance()
        {
            if(_isActive && HasMultipleActiveInstance())
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "More than one CameraMotion setting is active");
            }
        }
    }
}
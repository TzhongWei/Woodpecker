using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.Control.Camera;


namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    /// <summary>
    /// Camera Goo component.
    /// </summary>
    public class GH_CameraGoo : GH_Goo<Plane>
    {
        private Plane CameraPlane => this.Value;
        private CameraParameter _cameraParameter;
        public CameraParameter CameraValue => _cameraParameter;
        
        public GH_CameraGoo()
        {}
        public GH_CameraGoo(CameraParameter value)
        {
            this.Value = new Plane(value.CameraLocation, value.viewportInfo.CameraX, value.viewportInfo.CameraY);
            
            this._cameraParameter = value;
        }
        public override bool IsValid => _cameraParameter != null && Value.IsValid;
        public override string TypeName => "Camera Param Goo";
        public override string TypeDescription => "Camera viewport info parameters";
        public override IGH_Goo Duplicate()
        {
            /// not deep copy
            return new GH_CameraGoo(_cameraParameter);
        }
        public override string ToString()
        {
            return this._cameraParameter?.Name ?? "Null Camera";
        }
        public override bool CastFrom(object source)
        {
            if(source is GH_CameraGoo cameraGoo)
            {
                this.Value = cameraGoo.CameraPlane;
                this._cameraParameter = cameraGoo.CameraValue;
                return true;
            }
            if(source is CameraParameter camera)
            {
                this.Value = new Plane(camera.CameraLocation, camera.CameraDirection);
                this._cameraParameter = camera;
                return true;
            }
            if(source is GH_String GH_Name)
            {
                try
                {
                    var cameraVal = new CameraParameter(GH_Name.Value);
                    this.Value = new Plane(cameraVal.CameraLocation, cameraVal.CameraDirection);
                    this._cameraParameter = cameraVal;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            if(source is string Name)
            {
                try
                {
                    var cameraVal = new CameraParameter(Name);
                    this.Value = new Plane(cameraVal.CameraLocation, cameraVal.CameraDirection);
                    this._cameraParameter = cameraVal;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        public override bool CastTo<Q>(ref Q target)
        {
            if(typeof(Q).IsAssignableFrom(typeof(CameraParameter)) && this._cameraParameter != null)
            {
                object obj = this._cameraParameter;
                target = (Q)obj;
                return true;
            }
            if(typeof(Q).IsAssignableFrom(typeof(string)))
            {
                object obj = this._cameraParameter?.Name ?? string.Empty;
                target = (Q)obj;
                return true;
            }
            return base.CastTo<Q>(ref target);
        }
    }
}
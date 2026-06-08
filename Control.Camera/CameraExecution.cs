using System.Collections.Generic;

namespace Woodpecker.Animation.Control.Camera
{
    public class CameraExecution
    {
        private CameraMotionAbstract _cameraMotion;
        public CameraExecution(CameraMotionAbstract cameraMotion)
        {
            this._cameraMotion = cameraMotion;
        }
        public bool Execute(double t, bool applyCameraMotion = false)
        {
            if (_cameraMotion.IsInclude(t))
            {
                _cameraMotion.SetApplyCameraMotion(applyCameraMotion);
                bool result = false;
                if (_cameraMotion.PreEvaluate())
                {

                    _cameraMotion.Evaluate(t);
                    if(applyCameraMotion)
                    {
                        _cameraMotion.ApplyMotion(_cameraMotion.IsEnd(t));
                    }

                    result = _cameraMotion.PostEvaluate();

                }
                return result;
            }
            return false;
        }
    }
}
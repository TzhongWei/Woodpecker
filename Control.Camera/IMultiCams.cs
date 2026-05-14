using System.Collections.Generic;

namespace Woodpecker.Animation.Control.Camera
{
    public interface IMultiCamsTransform
    {
        List<CameraMotionAbstract> CamerasTS { get; }
        CameraParameter EvaluateMulti(double t);
    }
}
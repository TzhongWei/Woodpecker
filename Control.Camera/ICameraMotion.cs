namespace Woodpecker.Animation.Control.Camera
{
    public interface ICameraMotion
    {
        CameraParameter KeyCamera { get;}
        /// <summary>
        /// The camera that is being manipulated by the motion. This is the camera that will be evaluated and returned in the Evaluate function. 
        /// It can be the same as StartCamera but a phantom mode, or it can be a separate camera that is used as a reference for the motion. 
        /// For example, in a look-at motion, the MotionCamera can be a phantom camera that is used to calculate the look-at transformation, 
        /// while the KeyCamera is the camera that is being transformed. The MotionCamera can also be used to store intermediate camera parameters 
        /// during the motion, such as in a keyframe animation where the MotionCamera is updated at each keyframe and then evaluated at any time t.
        /// </summary>
        CameraParameter MotionCamera { get; set;}
        CameraParameter Evaluate(double t);
        bool PreEvaluate();
        bool PostEvaluate();
    }
}
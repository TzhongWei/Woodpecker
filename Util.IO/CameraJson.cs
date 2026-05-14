using System;
using System.Collections.Generic;
using Rhino;
using Rhino.FileIO;
using Rhino.Display;
using Rhino.Geometry;

namespace Woodpecker.Animation.Util.IO
{
    /// <summary>
    /// Save camera value into Json file. 
    /// </summary>
    public static class CameraJson
    {
        public struct perspectiveCameraInformation
        {
            string CameraName;
            Vector3d CameraDirection;
            Point3d CameraLocation;
        }
        public struct parallelCameralInformation
        {
            string CameraName;
        }

    }
}
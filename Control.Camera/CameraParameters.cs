using System.Collections.Generic;
using System.Linq;
using System;
using Rhino.DocObjects;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.Geometry.Display;
using Rhino.Display;
using Rhino;
using System.Drawing;

namespace Woodpecker.Animation.Control.Camera
{
    public enum CameraReference
    {
        RhinoReference,
        Phantom,
    }
    
    public class CameraParameter
    {
        public readonly CameraReference SourceType;
        public string Name { get; set; }
        private ViewInfo _viewInfo;
        public ViewInfo Info => this._viewInfo;
        private ViewportInfo _viewportInfo;
        public ViewportInfo viewportInfo => this._viewportInfo;
        public readonly int ViewIndex;
        public Vector3d CameraDirection => this._viewportInfo.CameraDirection;
        public Vector3d CameraUp => this._viewportInfo.CameraUp;
        public Point3d CameraTarget => this._viewportInfo.TargetPoint;
        public Point3d CameraLocation => this._viewportInfo.CameraLocation;
        public bool IsParallel => this._viewportInfo.IsParallelProjection;
        public double CameraLength => this._viewportInfo.Camera35mmLensLength;
        public Rectangle WindowRect;
        public Rectangle BaseWindowRect {get; private set;}
        public void SetParallel(bool ChangeToParallel)
        {
            if (IsParallel == ChangeToParallel)
                return;
            if (ChangeToParallel)
                this._viewportInfo.ChangeToParallelProjection(true);
            else
                this._viewportInfo.ChangeToPerspectiveProjection(CameraTarget.DistanceTo(CameraLocation), true, CameraLength);
        }
        /// <summary>
        /// Initialise a camera parameter by referencing an existing named view in Rhino. The named view must exist in the Rhino document, and it will be used as the KeyCamera for the motion. The camera parameters will be updated and evaluated based on the transformations applied to this camera. The named view will also be updated in Rhino when the motion is evaluated, so it can be used to visualize the camera movement in Rhino. If you want to create a phantom camera that is not linked to a named view in Rhino, you can use the other constructor that takes a ViewportInfo directly.
        /// </summary>
        /// <param name="Name"></param>
        /// <exception cref="Exception"></exception>
        public CameraParameter(string Name)
        {
            var doc = Rhino.RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                throw new Exception("RhinoDoc.ActiveDoc cannot be null");
            }
            var index = doc.NamedViews.FindByName(Name);
            if (index < 0)
            {
                throw new Exception($"viewport {Name} cannot be found in rhino");
            }
            this.ViewIndex = index;
            this._viewInfo = doc.NamedViews[index];
            this.Name = this._viewInfo.Name;
            _viewportInfo = new ViewportInfo(this._viewInfo.Viewport);
            SourceType = CameraReference.RhinoReference;
            WindowRect = CameraUtil.ViewRect(_viewportInfo);
            BaseWindowRect = WindowRect;
        }
        /// <summary>
        /// Initialise a phantom camera parameter with the given viewport info. The viewport info can be from a rhino viewport or a custom viewport info.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="viewportInfo"></param>
        public CameraParameter(string Name, ViewportInfo viewportInfo)
        {
            this.Name = Name;
            _viewportInfo = new ViewportInfo(viewportInfo);
            this.ViewIndex = -1;
            SourceType = CameraReference.Phantom;
            WindowRect = CameraUtil.ViewRect(_viewportInfo);
            BaseWindowRect = WindowRect;
        }
        public static CameraParameter CreateCameraParameter(string Name, RhinoViewport rhinoViewport, bool CreateInRhino = true)
        {
            if (CreateInRhino)
            {
                var _viewInfo = new ViewInfo(rhinoViewport);
                _viewInfo.Name = Name;
                var doc = Rhino.RhinoDoc.ActiveDoc;
                if (doc == null)
                {
                    throw new Exception("RhinoDoc.ActiveDoc cannot be null");
                }
                doc.NamedViews.Add(_viewInfo);
                return new CameraParameter(Name);
            }
            else
            {
                return new CameraParameter(Name, new ViewportInfo(rhinoViewport));
            }
        }
        public static CameraParameter CreateCameraParameter(string Name, ViewportInfo viewportInfo, bool CreateInRhino = true)
        {
            if(CreateInRhino)
            {
                var doc = Rhino.RhinoDoc.ActiveDoc;
                if (doc == null)
                {
                    throw new Exception("RhinoDoc.ActiveDoc cannot be null");
                }
                var activeView = doc.Views.ActiveView;
                var copy_viewportInfo = new ViewportInfo(viewportInfo);
                var tempViewIndex = doc.NamedViews.Add("__temp_view__", activeView.ActiveViewportID);

                activeView.ActiveViewport.SetViewProjection(copy_viewportInfo, false);
                doc.NamedViews.Add(Name, activeView.ActiveViewport.Id);
                if(tempViewIndex >= 0)
                {
                    doc.NamedViews.Restore(tempViewIndex, activeView.ActiveViewport);
                    doc.NamedViews.Delete(tempViewIndex);
                }
                

                return new CameraParameter(Name);
            }
            else
            {
                return new CameraParameter(Name, viewportInfo);
            }
        }
        public CameraParameter Duplicate(string newName, bool CreateInRhino = false)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception("newName cannot be null or empty.");
            var duplicate = CreateCameraParameter(newName, this._viewportInfo, CreateInRhino);
            duplicate.WindowRect = this.WindowRect;
            duplicate.BaseWindowRect = this.BaseWindowRect;
            return duplicate;
        }
        public CameraParameter ToPhantom(string name = null)
        {
            var phantom = new CameraParameter(name ?? this.Name, this._viewportInfo);
            phantom.WindowRect = this.WindowRect;
            phantom.BaseWindowRect = this.BaseWindowRect;
            return phantom;
        }
        public void ShowCameraWire(GH_DocumentObject docObj, IGH_PreviewArgs args)
        {
            var lines = CameraUtil.DisplayCamera(this);
            var selected = docObj.Attributes.Selected ? DisplayDefaultColour.SelectedColour : DisplayDefaultColour.UnSelectedColour;
            foreach (var lineCrv in lines)
            {
                args.Display.DrawCurve(lineCrv, selected);
            }
            var dirLines = CameraUtil.DisplayCameraDirection(this._viewportInfo);
            var XYZColour = new List<Color>();
            XYZColour.Add(docObj.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.FromArgb(255, 255, 0, 0));
            XYZColour.Add(docObj.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.FromArgb(255, 0, 255, 0));
            XYZColour.Add(docObj.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.FromArgb(255, 0, 0, 255));

            for (int i = 0; i < dirLines.Count; i++)
            {
                args.Display.DrawArrow(dirLines[i], XYZColour[i]);
            }

            var windowsRect = CameraUtil.GetTargetRectCorners(this);
            var indices = new int[] { 0, 1, 3, 2, 0 };
            var polyRec = new PolylineCurve(indices.Select(i => windowsRect[i]));
            var hiddenline = DashType.Hidden;
            var curveDisplay = new CurveDisplay(polyRec, hiddenline);
            var hiddenCurves = curveDisplay.GetCurvesByDashType();
            var hiddenlineColor = docObj.Attributes.Selected ? DisplayDefaultColour.SelectedColour : Color.YellowGreen;
            foreach (var crv in hiddenCurves)
            {
                args.Display.DrawCurve(crv, hiddenlineColor);
            }
        }
    }
}

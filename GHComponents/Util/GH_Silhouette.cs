
using System;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Extract silhouette outline curves from geometry for display or drafting. Inputs include Geometry. Outputs include Outline.
    /// </summary>
    public class GH_Silhouette: GH_Component
    {
        public override Guid ComponentGuid => new Guid("6d90bdf5-af93-4371-87a9-0d11d89ae8a6");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry from which to compute viewport silhouette outlines.", GH_ParamAccess.item);
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Silhouette;

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Outline", "OL", "Silhouette outline curves for the input geometry.", GH_ParamAccess.list);
        }
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(
                menu,
                "Update By View",
                updatebyView,
                true,
                _update_By_View);
        }

        private bool _update_By_View;
        private bool _viewUpdateScheduled;
        private bool _viewEventsSubscribed;
        private Vector3d _lastCameraDirection = Vector3d.Unset;
        private const double CameraDirectionTolerance = 1e-6;

        private void updatebyView(object sender, EventArgs e)
        {
            _update_By_View = !_update_By_View;

            if (_update_By_View)
            {
                _lastCameraDirection = Vector3d.Unset;
                SubscribeViewEvents();
            }
            else
            {
                UnsubscribeViewEvents();
                _viewUpdateScheduled = false;
            }

            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("UpdateByView", _update_By_View);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("UpdateByView", ref _update_By_View);
            _lastCameraDirection = Vector3d.Unset;
            _viewUpdateScheduled = false;
            return base.Read(reader);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase G = null;
            if (!DA.GetData("Geometry", ref G) ||
                G == null ||
                !G.IsValid)
            {
                return;
            }

            if (!TryGetActiveCameraDirection(out var cameraDirection))
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Warning,
                    "Cannot find an active Rhino viewport for silhouette calculation.");
                return;
            }

            _lastCameraDirection = cameraDirection;
            var Crvs = DisplayUtil.DisplaySilhouette(
                G,
                cameraDirection);

            DA.SetDataList("Outline", Crvs);
        }

        public GH_Silhouette():base("Silhouette", "Sil", "Extract silhouette outline curves from geometry for display or drafting.", "Woodpecker", "Util"){}

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            if (_update_By_View)
            {
                SubscribeViewEvents();
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            UnsubscribeViewEvents();
            base.RemovedFromDocument(document);
        }

        private static bool TryGetActiveCameraDirection(
            out Vector3d cameraDirection)
        {
            cameraDirection = Vector3d.Unset;

            var activeView = RhinoDoc.ActiveDoc?.Views.ActiveView;
            if (activeView == null)
                return false;

            cameraDirection = activeView.ActiveViewport.CameraDirection;
            return cameraDirection.IsValid &&
                   cameraDirection.Unitize();
        }

        private bool HasActiveViewChanged()
        {
            if (!TryGetActiveCameraDirection(out var currentDirection))
                return false;

            if (!_lastCameraDirection.IsValid)
                return true;

            return (currentDirection - _lastCameraDirection).Length >
                   CameraDirectionTolerance;
        }

        private void SubscribeViewEvents()
        {
            if (_viewEventsSubscribed)
                return;

            Rhino.Display.RhinoView.Modified += RhinoViewChanged;
            Rhino.Display.RhinoView.SetActive += RhinoViewChanged;
            _viewEventsSubscribed = true;
        }

        private void UnsubscribeViewEvents()
        {
            if (!_viewEventsSubscribed)
                return;

            Rhino.Display.RhinoView.Modified -= RhinoViewChanged;
            Rhino.Display.RhinoView.SetActive -= RhinoViewChanged;
            _viewEventsSubscribed = false;
        }

        private void RhinoViewChanged(
            object sender,
            Rhino.Display.ViewEventArgs e)
        {
            if (!_update_By_View ||
                !HasActiveViewChanged())
            {
                return;
            }

            ScheduleViewUpdate();
        }

        private void ScheduleViewUpdate()
        {
            if (_viewUpdateScheduled)
                return;

            var doc = OnPingDocument();
            if (doc == null)
                return;

            _viewUpdateScheduled = true;
            doc.ScheduleSolution(1, scheduledDoc =>
            {
                _viewUpdateScheduled = false;
                ExpireSolution(false);
            });
        }
    }
}

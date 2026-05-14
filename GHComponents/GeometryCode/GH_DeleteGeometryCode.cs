using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.IO;


namespace Woodpecker.Animation.GHComponents
{
    public class GH_DeleteGeometryCode : GH_GeometryCodeAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public GH_DeleteGeometryCode() : base("Delete GeometryCodes", "DelGC", "Delete selected geometry code entries from the active geometry code book.") { }
        public override Guid ComponentGuid => new Guid("0a22e029-c33c-4ec2-8a9a-6c56ec4fbc9a");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CodeName", "Name", "The name of the geometry", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Delete", "D", "Whether the geometry code was successfully deleted", GH_ParamAccess.item);
        }
        private bool _delResult = false;
        private List<string> _delNames;
        private void _delToggle()
        {
            _delResult = true;
            foreach (var name in _delNames)
                _delResult &= ProjectAppManager.GCParameters.Values.Remove(name);

            var doc = this.OnPingDocument();
            if (doc == null) return;
            CodeManager.RefleshGHDocument.RefleshComponents(doc, this.UpdateTag);

        }
        public override void CreateAttributes()
        {
            this.m_attributes = new ButtonUIAttributes(this, "Delete", _delToggle, "Delete Geometry");
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _delResult = false;
            _delNames = new List<string>();
            DA.GetDataList("CodeName", _delNames);
            var errorName = new List<string>();
            foreach (var name in _delNames)
            {
                if (!ProjectAppManager.GCParameters.Labels.Contains(name))
                {
                    errorName.Add(name);
                }
            }

            if (errorName.Count > 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Join(", ", errorName) + " cannot be found in the file");
            }
            DA.SetData("Delete", _delResult);
        }
    }
}

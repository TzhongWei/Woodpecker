using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GH_IO.Serialization;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.IO;
using Grasshopper.Kernel.Data;
using Woodpecker.Animation.CodeManager;
using System.Windows.Forms;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_LoadGeometry : GH_GeometryCodeAbstract, ISingletonDocumentComponent, ISelectExistFile
    {
        public string SingletonTag => "LGeometryCode";
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public GH_LoadGeometry() : base("Load Geometry", "LG", "Load the geometry from the database") { }
        public override Guid ComponentGuid => new Guid("2cf7dc60-e623-495a-9d35-4b708f2862b8");
        public bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return true;

            var sameComponents = doc.Objects
                .OfType<GH_Component>()
                .Where(x => x.GetType() == this.GetType())
                .OrderBy(x => x.InstanceGuid)
                .ToList();

            if (sameComponents.Count == 0) return true;

            return sameComponents.First().InstanceGuid == this.InstanceGuid;
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("GeometryCodePath", "GP", "Path to the geometry code file. If empty string is provided, use default", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GeometryName", "GN", "The name of the geometry object", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Geometry", "GB", "The objects read from the path", GH_ParamAccess.tree);
            pManager.AddTextParameter("GeometryCodeTree", "GC", "The geometry code includes object name and form information", GH_ParamAccess.tree);
        }
        public override void CreateAttributes()
        {
            m_attributes = new ButtonUIAttributesEditable(this, "Reflesh", After_Select_RefreshComponent, ShowEditor, "Undate geometry code");
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Select a Colour code json", Menu_SelectExistingFileClicked);
        }
        public void ShowEditor()
        {
            Select_SingleExistingFileClicked();
        }
        public void Menu_SelectExistingFileClicked(object sender, EventArgs e)
        {
            Select_SingleExistingFileClicked();
        }
        public void After_Select_RefreshComponent()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return;
            CodeManager.RefleshGHDocument.RefleshComponents(doc, this.UpdateTag);
        }
        public void Select_SingleExistingFileClicked()
        {
            SelectExistFileExtensions.Select_SingleExistingFileClicked(this, "Select a geometry code file");
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!IsPrimaryInstance())
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                "Another Load Geometry component is already active in this document. This instance is locked.");
                return;
            }


            var path = "";
            if (DA.GetData("GeometryCodePath", ref path))
            {
                GeometryCodeIO.ReadGeometryFromPath(path);
            }
            if (ProjectAppManager.GCParameters == null)
            {
                GeometryCodeIO.SetDefaultGeometryCode();
            }
            var geomparam = ProjectAppManager.GCParameters.Values;
            var names = new DataTree<string>();
            var geomsTree = new DataTree<GeometryBase>();
            var ind = 0;
            foreach (var kvp in geomparam)
            {
                names.Add(kvp.Key, new GH_Path(ind));
                geomsTree.AddRange(kvp.Value.Select(x => (GeometryBase)x), new GH_Path(ind));
            }
            DA.SetDataTree(0, names);
            DA.SetDataTree(1, geomsTree);
            DA.SetDataTree(2, ProjectAppManager.GCParameters.To_GH_DataTree());
        }
    }

    [Obsolete]
    public class GH_LoadGeometry_Old : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public GH_LoadGeometry_Old() : base("LoadGeometry", "LG", "Load the geometry from the database", "Woodpecker", "Util")
        {

        }

        public override Guid ComponentGuid => new Guid("01550cfc-ad27-4d7d-978d-59e362287aff");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Refresh", "R", "Refresh this component", GH_ParamAccess.item);
            pManager.AddTextParameter("GeometryPath", "GP", "Path to the Geometry file. If -1 is provided, use default", GH_ParamAccess.item, "./data/GeometryData.json");
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("SGeometry", "SG", "Geometry from the saved database", GH_ParamAccess.item);
        }
        private int _selectedItem = 0;
        private List<string> _selectionList = new List<string>();
        public Dictionary<string, GeometryBase> SetSelectionList(ref string path)
        {
            if (path == "-1" || string.IsNullOrWhiteSpace(path))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "data");
                path = Path.Combine(dir, "GeometryData.json");
                GeometryCodeUtil.SetDefaultGeometryData(path);
            }
            if (!File.Exists(path))
            {
                GeometryCodeUtil.SetDefaultGeometryData(path);
            }
            if (GeometryCodeUtil.ReadGeometryFromJson(path, out var Geoms))
                return Geoms;
            else
                return null;
        }
        private string _path;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            bool refresh = false;
            DA.GetData("Refresh", ref refresh);
            DA.GetData("GeometryPath", ref path);

            var GeoDic = SetSelectionList(ref path);
            if (GeoDic == null)
            {
                DA.SetData(0, null);
                return;
            }
            var newSelectionList = GeoDic.Keys.ToList();
            var listChanged = !_selectionList.SequenceEqual(newSelectionList);

            _selectionList = newSelectionList;

            if (_selectionList.Count == 0)
            {
                DA.SetData("SGeometry", null);
            }

            if (_selectedItem >= _selectionList.Count)
                _selectedItem = -1;

            if (refresh || listChanged)
            {
                this.Attributes?.ExpireLayout();
                ((ValueListUIAttributes)this.Attributes).UndateList(this._selectionList);
                this.OnDisplayExpired(true);
            }
            if (_selectedItem >= 0)
            {
                var selectedKey = _selectionList[_selectedItem];
                var selectedGeom = GeoDic[selectedKey];
                _path = path;
                DA.SetData(0, selectedGeom);
            }
            else
                DA.SetData(0, null);
        }

        public override void CreateAttributes()
        {
            // if (_selectionList.Count == 0)
            // {
            //     var geoDic = SetSelectionList("");

            //     if (geoDic == null || geoDic.Count == 0)
            //     {
            //         _selectionList = new List<string> { "No Geometry Option" };
            //     }
            //     else
            //     {
            //         _selectionList = geoDic.Keys.ToList();
            //     }
            // }
            try
            {
                m_attributes = new ValueListUIAttributes(this, OnSelected, _selectionList, "Selection List", -1);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);
                base.CreateAttributes();
            }
        }
        // public GeometryBase LoadedGeometry { get; private set; } Future implementation: store the loaded geometry in the component and provide it as an output, so that other components can access it without needing to read the file again.
        public void OnSelected(int index)
        {
            if (index < 0 || index >= _selectionList.Count)
                return;
            if (index == _selectedItem)
                return;
            _selectedItem = index;
            this.Attributes?.ExpireLayout();
            this.OnDisplayExpired(true);
            this.ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("SelectedIndex", _selectedItem);
            writer.SetString("Path", _path);
            /// Save the geometry data if the geometry is deleted from the document, so that it can be retrieved when the component is loaded again. --- IGNORE ---

            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            var index = -1;
            var path = "";
            if (reader.ItemExists("SelectedIndex"))
            {
                if (reader.TryGetInt32("SelectedIndex", ref index))
                {
                    _selectedItem = index;
                }
                if (reader.TryGetString("Path", ref path))
                {
                    _path = path;
                }
            }
            if (_selectedItem >= 0)
            {
                this.Attributes?.ExpireLayout();
                this._selectionList = this.SetSelectionList(ref path).Keys.ToList();
                ((ValueListUIAttributes)this.Attributes).UndateList(this._selectionList);
                ((ValueListUIAttributes)this.m_attributes).UpdateSelectedIndex(_selectedItem);
                this.OnDisplayExpired(true);
            }
            return base.Read(reader);
        }
    }
}
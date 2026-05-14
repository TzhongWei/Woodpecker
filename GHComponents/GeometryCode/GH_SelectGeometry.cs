using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.IO;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_SelectGeometry : GH_GeometryCodeAbstract
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public GH_SelectGeometry():base("Select Geometry", "SelG", "Select a list of geometry from the list")
        {
            
        }
        public override Guid ComponentGuid => new Guid("83f0620e-863b-4e77-8ffb-60c72d0086b7");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Refresh", "R", "Refresh this component, compulsorily", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Selected geometry objects from the geometry code list.", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool refresh = false;
            DA.GetData("Refresh", ref refresh);
            var geomparam = SetSelectionList();
            var newSelectionList = geomparam.Keys.ToList();
            bool listChanged = !_selectionList.SequenceEqual(newSelectionList);

            _selectionList = newSelectionList;

            if (_selectionList.Count == 0)
            {
                DA.SetDataList(0, new List<GeometryBase>());
            }


            if (_selectedItem >= _selectionList.Count)
                _selectedItem = -1;

            if (refresh || listChanged)
            {
                //CreateAttributes();
                this.Attributes?.ExpireLayout();
                ((ValueListUIAttributes)this.m_attributes).UndateList(this._selectionList);
                this.OnDisplayExpired(true);
            }
            if(!string.IsNullOrWhiteSpace(this._selectedName))
            {
                var foundIndex = _selectionList.FindIndex(x => x == _selectedName);
                if(foundIndex >= 0)
                    _selectedItem = foundIndex;
            }
            if (_selectedItem >= 0)
            {
                var selectedKey = _selectionList[_selectedItem];
                var selectedGeom = geomparam[selectedKey].Select(x => (GeometryBase)x).ToList();
                ((ValueListUIAttributes)this.m_attributes).UpdateSelectedIndex(_selectedItem);
                DA.SetDataList(0, selectedGeom);
            }
            else
                DA.SetDataList(0, new List<GeometryBase>());
        }
        private Dictionary<string, List<GeometryDataPair>> SetSelectionList()
        {
            if(ProjectAppManager.GCParameters == null || !ProjectAppManager.GCParameters.IsValid)
            {
                GeometryCodeIO.SetDefaultGeometryCode(); //will not save the geometry
            }
            return ProjectAppManager.GCParameters.Values;
        }
        private List<string> _selectionList = new List<string>();
        private string _selectedName = "";
        public override void CreateAttributes()
        {
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
        private int _selectedItem = -1;
        public void OnSelected(int index)
        {
            if (index < 0 || index >= _selectionList.Count)
                return;
            if (index == _selectedItem)
                return;
            _selectedItem = index;
            _selectedName = "";
            this.Attributes?.ExpireLayout();
            this.OnDisplayExpired(true);
            this.ExpireSolution(true);
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("SelectedIndex", _selectedItem);
            if(_selectedItem > 0)
                writer.SetString("SelectedName", _selectionList[_selectedItem]);
            
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            var index = -1;
            var name = string.Empty;
            bool foundbyName = false;
            bool returnVal = base.Read(reader);
            this._selectionList = SetSelectionList().Keys.ToList();
            if(reader.TryGetString("SelectedName", ref name))
            {
                var foundIndex = _selectionList.FindIndex(x => x == name);
                if(foundIndex >= 0)
                {
                    _selectedItem = foundIndex;
                    foundbyName = true;
                }
            }
            if (!foundbyName && reader.TryGetInt32("SelectedIndex", ref index))
            {
                if(index >= 0 && index < _selectionList.Count)
                    _selectedItem = index;

            }
            if (_selectedItem >= 0 && _selectedItem < _selectionList.Count)
            {
                this.Attributes?.ExpireLayout();
                ((ValueListUIAttributes)this.m_attributes).UndateList(this._selectionList);
                ((ValueListUIAttributes)this.m_attributes).UpdateSelectedIndex(_selectedItem);
                this.OnDisplayExpired(true);
            }
            return returnVal;
        }
    }
}


using Grasshopper;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using Woodpecker.Animation.Util.IO;
using System.Linq;
using System.Drawing;
using GH_IO.Serialization;
using Rhino.Input.Custom;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Select a colour from the list. Inputs include Refresh. Outputs include Colour.
    /// </summary>
    public class GH_SelectedColour : GH_ColourCodeAbstract
    {
        public override GH_Exposure Exposure =>  GH_Exposure.primary;
        public GH_SelectedColour():base("Select Colour", "SelectC", "Select a colour from the list"){}
        public override Guid ComponentGuid => new Guid("a68cd7c6-2c02-49bd-ba5b-2363aab47189");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GH_Colour_Sel;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Refresh", "R", "Refresh this component, compulsorily", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Colour", "C", "List of colours associated with the selected key from the colour code.", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool refresh = false;
            DA.GetData("Refresh", ref refresh);
            
            var colourDict = SetSelectionList();
            var newSelectionList = colourDict.Keys.ToList();
            bool listChanged = !_selectionList.SequenceEqual(newSelectionList);

            _selectionList = newSelectionList;

            if (_selectionList.Count == 0)
            {
                DA.SetDataList(0, new List<Color>());
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
                this._selectedName = _selectionList[_selectedItem];
                var selectedKey = _selectionList[_selectedItem];
                var selectedColours = colourDict[selectedKey];
                ((ValueListUIAttributes)this.m_attributes).UpdateSelectedIndex(_selectedItem);
                DA.SetDataList(0, selectedColours);
            }
            else
                DA.SetDataList(0, new List<Color>());
        }
        private List<string> _selectionList = new List<string>();
        private string _selectedName = "";
        private Dictionary<string, List<Color>> SetSelectionList()
        {
            if(ProjectAppManager.CCParameters == null || !ProjectAppManager.CCParameters.IsValid)
            {
                ColourCodeIO.SetDefaultColourCode(); //Would not save the colour
            }
            
            return ProjectAppManager.CCParameters.Values;
        }
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
    [Obsolete]
    /// <summary>
    /// Legacy component for selecting colours from a colour code file. Inputs include Refresh. Outputs include Colour.
    /// </summary>
    public class GH_SelectedColour_old : GH_Component
    {
        public override GH_Exposure Exposure =>  GH_Exposure.hidden;
        public GH_SelectedColour_old() : base("ColourSelected", "CS", "Legacy component for selecting colours from a colour code file.", "Woodpecker", "ColourCode") { }
        public override Guid ComponentGuid => new Guid("d6095e5b-c54d-4b7c-abfb-0ee67d02aace");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Refresh", "R", "Refresh this component", GH_ParamAccess.item, false);
            pManager.AddTextParameter("ColourCodePath", "CP", "Path to the colour code file. If -1 is provided, use default", GH_ParamAccess.item, "");
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("Colour", "C", "Colours associated with the selected colour code entry.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            bool refresh = false;
            DA.GetData("Refresh", ref refresh);
            DA.GetData("ColourCodePath", ref path);

            var colourDict = SetSelectionList(ref path);
            var newSelectionList = colourDict.Keys.ToList();
            bool listChanged = !_selectionList.SequenceEqual(newSelectionList);

            _selectionList = newSelectionList;

            if (_selectionList.Count == 0)
            {
                DA.SetDataList(0, new List<Color>());
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
            if (_selectedItem >= 0)
            {
                var selectedKey = _selectionList[_selectedItem];
                var selectedColours = colourDict[selectedKey];
                this._path = path;
                DA.SetDataList(0, selectedColours);
            }
            else
                DA.SetDataList(0, new List<Color>());
        }
        private List<string> _selectionList = new List<string>();
        private string _path = string.Empty;
        private Dictionary<string, List<Color>> SetSelectionList(ref string path)
        {
            if (path == "-1") //Run default
            {
                ColourCodeUtil.SetDefaultColourCode_Old();
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "data");

                path = Path.Combine(dir, "ColourCode.json");
            }
            //If no path is provided, fall back to the default colour code file.
            else if (string.IsNullOrWhiteSpace(path))
            {
                Woodpecker.Animation.Util.IO.ColourCodeUtil.SetDefaultColourCode("");
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "data");

                path = Path.Combine(dir, "ColourCode.json");
            }
            else if (!File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"The specified colour code file does not exist at path: {path}. Falling back to default colour code.");
                Woodpecker.Animation.Util.IO.ColourCodeUtil.SetDefaultColourCode(path);
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "data");

                path = Path.Combine(dir, "ColourCode.json");
            }
            var colorDic = ColourCodeUtil.GetColourCode(path);
            return colorDic;
        }
        public override void CreateAttributes()
        {
            // if (_selectionList.Count == 0) //Run Default
            // {
            //     _selectionList = SetSelectionList("").Keys.ToList();
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
        private int _selectedItem = -1;
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
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            var index = -1;
            var path = string.Empty;
            if (reader.ItemExists("SelectedIndex"))
            {
                if (reader.TryGetInt32("SelectedIndex", ref index))
                {
                    _selectedItem = index;
                }
                if(reader.TryGetString("Path", ref path))
                {
                    _path = path;
                }
            }
            if (_selectedItem >= 0)
            {
                this.Attributes?.ExpireLayout();
                this._selectionList = this.SetSelectionList(ref _path).Keys.ToList();
                ((ValueListUIAttributes)this.m_attributes).UndateList(this._selectionList);
                ((ValueListUIAttributes)this.m_attributes).UpdateSelectedIndex(_selectedItem);
                this.OnDisplayExpired(true);
            }
            return base.Read(reader);
        }
    }
}

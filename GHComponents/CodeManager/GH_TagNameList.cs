using System;
using System.Collections.Generic;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Woodpecker.Animation.GHComponents;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.CodeManager
{
    /// <summary>
    /// Lists the distinct names of available input tag channels and outputs
    /// the tag selected from the component's embedded value list.
    /// </summary>
    public class GH_TagNameList : GH_TagChannel_Abstract
    {
        public override Guid ComponentGuid => new Guid("c63a42d2-d19c-48d3-8f02-bd4b9dbee6b6");
        public GH_TagNameList()
            : base(
                "Tag Name List",
                "TagList",
                "Select a tag from the input tag channels available in the current Grasshopper document.")
        {
        }

        public override RemoteType ChannelType => RemoteType.Process;

        public override bool IsPrimaryInstance()
        {
            return true;
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter(
                "Refresh",
                "R",
                "Refresh the list of available input tag channels.",
                GH_ParamAccess.item,
                false);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(
                "Tag Name",
                "Tag",
                "The selected tag name.",
                GH_ParamAccess.item);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ValueListUIAttributes(
                this,
                OnSelect,
                _tagNames,
                "Select a Tag Name",
                -1);
        }

        private readonly List<string> _tagNames = new List<string>();
        private int _selectedIndex = -1;
        private string _selectedTagName = string.Empty;
        private bool _listInitialised;
        private bool _previousRefresh;
        private bool _postSolutionRefreshScheduled;

        private void RefreshTagNames()
        {
            var doc = this.OnPingDocument();
            if (doc == null)
                return;

            var selectedTag = !string.IsNullOrWhiteSpace(_selectedTagName)
                ? _selectedTagName
                : _selectedIndex >= 0 &&
                  _selectedIndex < _tagNames.Count
                    ? _tagNames[_selectedIndex]
                    : null;

            var availableTags = doc.Objects
                .OfType<GH_TagChannel_Abstract>()
                .Where(component =>
                    component.InstanceGuid != InstanceGuid &&
                    component.ChannelType == RemoteType.Input)
                .Select(component => component.SingletonTag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(tag => tag, StringComparer.Ordinal)
                .ToList();

            _tagNames.Clear();
            _tagNames.AddRange(availableTags);

            if (!string.IsNullOrWhiteSpace(selectedTag))
            {
                _selectedIndex = _tagNames.FindIndex(tag =>
                    string.Equals(
                        tag,
                        selectedTag,
                        StringComparison.Ordinal));
            }

            if (_selectedIndex < 0 || _selectedIndex >= _tagNames.Count)
                _selectedIndex = _tagNames.Count > 0 ? 0 : -1;

            _selectedTagName =
                _selectedIndex >= 0 &&
                _selectedIndex < _tagNames.Count
                    ? _tagNames[_selectedIndex]
                    : string.Empty;

            if (Attributes is ValueListUIAttributes attributes)
            {
                attributes.UndateList(_tagNames);
                attributes.UpdateSelectedIndex(_selectedIndex);
            }

            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
            _listInitialised = true;
        }

        private void SchedulePostSolutionRefresh()
        {
            if (_postSolutionRefreshScheduled)
                return;

            var document = OnPingDocument();
            if (document == null)
                return;

            _postSolutionRefreshScheduled = true;
            document.ScheduleSolution(1, scheduledDocument =>
            {
                RefreshTagNames();
                ExpireSolution(false);
            });
        }

        private void OnSelect(int select)
        {
            if (select < 0 || select >= _tagNames.Count)
                return;

            if (_selectedIndex == select)
                return;

            _selectedIndex = select;
            _selectedTagName = _tagNames[select];
            Attributes?.ExpireLayout();
            OnDisplayExpired(true);
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            var selectedTag =
                _selectedIndex >= 0 &&
                _selectedIndex < _tagNames.Count
                    ? _tagNames[_selectedIndex]
                    : _selectedTagName;

            if (!string.IsNullOrWhiteSpace(selectedTag))
                writer.SetString("SelectedTagName", selectedTag);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var result = base.Read(reader);
            var selectedTag = string.Empty;

            if (reader.TryGetString("SelectedTagName", ref selectedTag))
                _selectedTagName = selectedTag?.Trim() ?? string.Empty;

            _selectedIndex = -1;
            _listInitialised = false;
            _postSolutionRefreshScheduled = false;

            return result;
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var refresh = false;
            DA.GetData("Refresh", ref refresh);

            var refreshRequested = refresh && !_previousRefresh;
            _previousRefresh = refresh;

            if (!_listInitialised)
            {
                RefreshTagNames();
                SchedulePostSolutionRefresh();
            }
            else if (refreshRequested)
            {
                RefreshTagNames();
            }

            if (_selectedIndex < 0 || _selectedIndex >= _tagNames.Count)
                return;

            DA.SetData(0, _tagNames[_selectedIndex]);
        }
    }
}

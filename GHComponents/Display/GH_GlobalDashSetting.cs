using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.GHComponents.CustomGHComponents;

namespace Woodpecker.Animation.GHComponents
{
    public class GH_GlobalDashSetting : GH_Component, ISingletonDocumentComponent, IEditableWindow
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public string SingletonTag { get; private set; }
        private const string DefaultTag = "GlobalDash";
        public bool IsPrimaryInstance()
        {
            var doc = this.OnPingDocument();
            if (doc == null) return false;

            var sameTag = doc.Objects.OfType<GH_GlobalDashSetting>()
            .Where(x => x.SingletonTag == this.SingletonTag)
            .OrderBy(x => x.InstanceGuid).ToList();

            return sameTag.Count == 1;
        }
        public GH_GlobalDashSetting() : base("Dash Pattern ValueList", "DP VList", "", "Woodpecker", "Display") { }
        public override Guid ComponentGuid => new Guid("e4474246-ca96-44d0-8544-5f58e277ef3a");
        private readonly List<DashTypeListItem> _valueListItems = new List<DashTypeListItem>();
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Tag Name", "Tag", "", GH_ParamAccess.item, DefaultTag);
            pManager.AddBooleanParameter("Change By Viewport", "CBV", "", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dash Pattern", "Pattern", "Selected dash pattern as a space-separated string.", GH_ParamAccess.item);
        }
        public override void CreateAttributes()
        {
            try
            {
                m_attributes = new ValueListUIAttributesEditable<DashTypeListItem>(this, OnSelect, this.ShowEditor, this._valueListItems, "Dash Type");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);
                base.CreateAttributes();
            }
        }
        private void OnSelect(int index)
        {
            this._sel = index;
            this.ExpireSolution(true);
        }
        private int _sel = -1;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            bool changeByViewPort = false;
            if (!DA.GetData("Tag Name", ref name))
            {
                return;
            }
            DA.GetData("Change By Viewport", ref changeByViewPort);

            //Call a new component or other component
            if(this.SingletonTag != name)
            {
                this._valueListItems.Clear();
                this.SingletonTag = name;
            }

            this.SingletonTag = string.IsNullOrWhiteSpace(name) ? DefaultTag : name;


            if (IsPrimaryInstance())
            {
                /// if this is a new tag component
                if(this._valueListItems.Count == 0)
                    SetListItems(CreateDefaultDashTypes(this.SingletonTag, changeByViewPort));
            }
            else
            {
                var doc = OnPingDocument();
                var primary = doc?.Objects.OfType<GH_GlobalDashSetting>()
                    .Where(x => x.SingletonTag == this.SingletonTag && x.InstanceGuid != this.InstanceGuid)
                    .OrderBy(x => x.InstanceGuid)
                    .FirstOrDefault();

                if (primary != null)
                {
                    SetListItems(primary._valueListItems);
                }
            }

            if(this._valueListItems.Count == 0)
                    SetListItems(CreateDefaultDashTypes(this.SingletonTag, changeByViewPort));

            if (_sel < 0 || _sel > _valueListItems.Count - 1)
            {
                return;
            }
            else
            {
                var selected = _valueListItems[_sel];

                DA.SetData("Dash Pattern", selected.DashPattern);
            }
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("SingletonTag", SingletonTag ?? DefaultTag);
            writer.SetInt32("SelectedIndex", _sel);
            writer.SetString("DashTypeListItems", JsonConvert.SerializeObject(_valueListItems.Select(ListItemState.FromItem).ToList()));
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            var tag = DefaultTag;
            if (reader.TryGetString("SingletonTag", ref tag))
                SingletonTag = tag;

            var selectedIndex = 0;
            if (reader.TryGetInt32("SelectedIndex", ref selectedIndex))
                _sel = selectedIndex;

            var listJson = "";
            if (reader.TryGetString("DashTypeListItems", ref listJson))
            {
                try
                {
                    var states = JsonConvert.DeserializeObject<List<ListItemState>>(listJson);
                    if (states != null)
                        SetListItems(states.Select(x => x.ToItem()));
                }
                catch
                {
                    SetListItems(CreateDefaultDashTypes(SingletonTag ?? DefaultTag, false));
                }
            }
            return base.Read(reader);
        }

        private void SetListItems(IEnumerable<DashTypeListItem> items)
        {
            _valueListItems.Clear();
            if (items != null)
            {
                var newList = items.Select(x => x.CloneDashType());
                _valueListItems.AddRange(newList);
            }
            if (m_attributes is ValueListUIAttributesEditable<DashTypeListItem> attributes)
                attributes.UpdateSelectedIndex(_sel);
        }

        private static List<DashTypeListItem> CreateDefaultDashTypes(string tag, bool Scale)
        {
            var dashCodeParam = new DashCodeParam(tag);
            dashCodeParam.Add(DashType.Continuous);
            dashCodeParam.Add(DashType.Dot);
            dashCodeParam.Add(DashType.DashDot);
            dashCodeParam.Add(DashType.Dashed);
            dashCodeParam.Add(DashType.Hidden);
            dashCodeParam.Set_ScalebyScreen(Scale);
            return DashTypeListItem.SetDashTypeList(dashCodeParam);
        }
        private class ListItemState
        {
            public string Name { get; set; }
            public string Expression { get; set; }
            public bool Selected { get; set; }
            public bool ScaleByScreen {get; set;}
            public static ListItemState FromItem(DashTypeListItem item)
            {
                return new ListItemState
                {
                    Name = item.Name,
                    Expression = item.Expression,
                    Selected = item.Selected,
                    ScaleByScreen = item.ScalebyScreen
                };
            }

            public DashTypeListItem ToItem()
            {
                var item = new DashTypeListItem(Name, Expression, ScaleByScreen);
                item.Selected = Selected;
                return item;
            }
        }
        private void UpdateSameTag()
        {
            if(!IsPrimaryInstance())
            {
                var doc = OnPingDocument();
                var sameComponents = doc?.Objects.OfType<GH_GlobalDashSetting>()
                    .Where(x => x.SingletonTag == this.SingletonTag && x.InstanceGuid != this.InstanceGuid)
                    .OrderBy(x => x.InstanceGuid).ToList();

                foreach(var component in sameComponents)
                {
                    component.SetListItems(this._valueListItems);
                    component.ExpireSolution(true);
                }
            }
        }
        public void ShowEditor()
        {
            var isScreenScale = _valueListItems.First().ScalebyScreen;
            var selectedList = this._valueListItems.Select(x => x.Selected).ToList();
            var editorText = string.Join(
                Environment.NewLine,
                _valueListItems.Select(x => $"{x.Name}={x.Expression}")
            );
            
            var dialog = new Eto.Forms.Dialog<bool>
            {
                Title = "Edit Value List",
                ClientSize = new Eto.Drawing.Size(420, 320),
                Resizable = true,
                Padding = new Eto.Drawing.Padding(10)
            };

            var textArea = new Eto.Forms.TextArea
            {
                Text = editorText
            };

            var okButton = new Eto.Forms.Button { Text = "OK" };
            var cancelButton = new Eto.Forms.Button { Text = "Cancel" };

            okButton.Click += (sender, args) => dialog.Close(true);
            cancelButton.Click += (sender, args) => dialog.Close(false);

            var layout = new Eto.Forms.DynamicLayout
            {
                Spacing = new Eto.Drawing.Size(5, 5)
            };
            layout.Add(textArea, yscale: true);
            layout.AddSeparateRow(null, cancelButton, okButton);

            dialog.Content = layout;

            var result = dialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (!result)
                return;

            var newItems = new List<DashTypeListItem>();
            var lines = textArea.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                    continue;

                var item = new DashTypeListItem(parts[0].Trim(), parts[1].Trim(), isScreenScale);
                newItems.Add(item);
            }

            this.OnPingDocument()?.UndoUtil.RecordGenericObjectEvent("Value List Change", this);
            
            if(newItems.SequenceEqual(_valueListItems))
                return;


            this.SetListItems(newItems);

            for (int i = 0; i < Math.Min(selectedList.Count, _valueListItems.Count); i++)
                _valueListItems[i].Selected = selectedList[i];
            
            if (this.Attributes is ValueListUIAttributesEditable<DashTypeListItem> attributes)
            {
                attributes.UpdateValueList(_valueListItems);
                attributes.UpdateSelectedIndex(_sel);
                attributes.ExpireLayout();
            }

            UpdateSameTag();
            this.OnDisplayExpired(true);

            this.ExpireSolution(true);



            /*
            var selectedList = _valueListItems.Select(x => x.Selected).ToList();
            var editorText = string.Join(
                Environment.NewLine,
                _valueListItems.Select(x => $"{x.Name}={x.Expression}")
            );

            var dialog = new Eto.Forms.Dialog<bool>
            {
                Title = "Edit Value List",
                ClientSize = new Eto.Drawing.Size(420, 320),
                Resizable = true,
                Padding = new Eto.Drawing.Padding(10)
            };

            var textArea = new Eto.Forms.TextArea
            {
                Text = editorText
            };

            var okButton = new Eto.Forms.Button { Text = "OK" };
            var cancelButton = new Eto.Forms.Button { Text = "Cancel" };

            okButton.Click += (sender, args) => dialog.Close(true);
            cancelButton.Click += (sender, args) => dialog.Close(false);

            var layout = new Eto.Forms.DynamicLayout
            {
                Spacing = new Eto.Drawing.Size(5, 5)
            };
            layout.Add(textArea, yscale: true);
            layout.AddSeparateRow(null, cancelButton, okButton);

            dialog.Content = layout;

            var result = dialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (!result)
                return;

            var newItems = new List<T>();
            var lines = textArea.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                    continue;

                var item = new T();
                item.SetValueList(parts[0].Trim(), parts[1].Trim());
                newItems.Add(item);
            }

            base.Owner.OnPingDocument()?.UndoUtil.RecordGenericObjectEvent("Value List Change", base.Owner);
            _valueListItems.Clear();
            _valueListItems.AddRange(newItems);

            for (int i = 0; i < Math.Min(selectedList.Count, _valueListItems.Count); i++)
                _valueListItems[i].Selected = selectedList[i];

            _updateList(_valueListItems.Select(x => x.Clone() as T).ToList());
            base.Owner.ExpireSolution(true);
            */
        }
    }
}

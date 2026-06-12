using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Geometry;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates annotation display settings for screen-facing or plane-oriented text.
    /// </summary>
    public sealed class GH_AnnotationSetting : GH_Component
    {
        private const string HorizontalListName = "Annotation Horizontal Alignment";
        private const string VerticalListName = "Annotation Vertical Alignment";

        private RenderTagMode _mode = RenderTagMode.OnWindow;

        public GH_AnnotationSetting()
            : base(
                "Annotation Setting",
                "AnnoSet",
                "Create display settings for screen-facing or plane-oriented annotations.",
                "Woodpecker",
                "Display")
        {
            UpdateMessage();
        }

        public override Guid ComponentGuid =>
            new Guid("772eb945-45c8-49d1-bcf7-7470d26a08d8");

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter(
                "Height",
                "H",
                "Text height in pixels for OnWindow mode or model units for OnPlane mode.",
                GH_ParamAccess.item,
                14);

            pManager.AddTextParameter(
                "FontFace",
                "Font",
                "Font family used to display the annotation.",
                GH_ParamAccess.item,
                "Arial");

            pManager.AddBooleanParameter(
                "MiddleJustified",
                "Middle",
                "Centre the screen-facing text around its location.",
                GH_ParamAccess.item,
                true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Annotation Setting",
                "S",
                "Annotation display setting.",
                GH_ParamAccess.item);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);

            Menu_AppendItem(
                menu,
                "Tag.OnWindow",
                (sender, args) => SetMode(RenderTagMode.OnWindow),
                true,
                _mode == RenderTagMode.OnWindow);

            Menu_AppendItem(
                menu,
                "Tag.OnPlane",
                (sender, args) => SetMode(RenderTagMode.OnPlane),
                true,
                _mode == RenderTagMode.OnPlane);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            ScheduleAlignmentLists(document);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var height = 14;
            var fontFace = "Arial";

            DA.GetData(0, ref height);
            DA.GetData(1, ref fontFace);

            var setting = new AnnotationDisplaySetting
            {
                TagMode = _mode,
                Height = height,
                FontFace = string.IsNullOrWhiteSpace(fontFace) ? "Arial" : fontFace.Trim()
            };

            if (_mode == RenderTagMode.OnWindow)
            {
                var middleJustified = true;
                DA.GetData(2, ref middleJustified);
                setting.MiddleJustified = middleJustified;
            }
            else
            {
                var bold = false;
                var italic = false;
                var horizontal = TextHorizontalAlignment.Center.ToString();
                var vertical = TextVerticalAlignment.Middle.ToString();
                var plane = Plane.WorldXY;

                DA.GetData(2, ref bold);
                DA.GetData(3, ref italic);
                DA.GetData(4, ref plane);
                DA.GetData(5, ref horizontal);
                DA.GetData(6, ref vertical);

                setting.Bold = bold;
                setting.Italic = italic;
                setting.Displayplane = plane;

                if (!TryParseHorizontalAlignment(horizontal, out var horizontalAlignment))
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Warning,
                        $"Horizontal alignment '{horizontal}' is invalid. Centre is used.");
                    horizontalAlignment = TextHorizontalAlignment.Center;
                }

                if (!Enum.TryParse(vertical, true, out TextVerticalAlignment verticalAlignment))
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Warning,
                        $"Vertical alignment '{vertical}' is invalid. Middle is used.");
                    verticalAlignment = TextVerticalAlignment.Middle;
                }

                setting.HorizontalAlignment = horizontalAlignment;
                setting.VerticalAlignment = verticalAlignment;
            }

            DA.SetData(0, setting);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("AnnotationMode", (int)_mode);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var modeValue = (int)RenderTagMode.OnWindow;
            reader.TryGetInt32("AnnotationMode", ref modeValue);

            _mode = Enum.IsDefined(typeof(RenderTagMode), modeValue)
                ? (RenderTagMode)modeValue
                : RenderTagMode.OnWindow;

            RebuildModeInputs();
            UpdateMessage();
            return base.Read(reader);
        }

        private void SetMode(RenderTagMode mode)
        {
            if (_mode == mode)
                return;

            RecordUndoEvent("Change Annotation Setting Mode");
            RemoveAutomaticAlignmentLists();

            _mode = mode;
            RebuildModeInputs();
            UpdateMessage();

            Params.OnParametersChanged();
            Attributes?.ExpireLayout();
            OnDisplayExpired(false);
            ExpireSolution(true);

            ScheduleAlignmentLists(OnPingDocument());
        }

        private void RebuildModeInputs()
        {
            while (Params.Input.Count > 2)
                Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);

            if (_mode == RenderTagMode.OnWindow)
            {
                Params.RegisterInputParam(CreateBooleanParameter(
                    "MiddleJustified",
                    "Middle",
                    "Centre the screen-facing text around its location.",
                    true));
                return;
            }

            Params.RegisterInputParam(CreateBooleanParameter(
                "Bold",
                "B",
                "Draw plane-oriented text using a bold font.",
                false));

            Params.RegisterInputParam(CreateBooleanParameter(
                "Italic",
                "I",
                "Draw plane-oriented text using an italic font.",
                false));
            Params.RegisterInputParam(CreatePlaneParameter(
                "Plane",
                "PL",
                "Orient the text on the plane",
                Plane.WorldXY
            ));

            Params.RegisterInputParam(CreateTextParameter(
                "HorizontalAlignment",
                "HA",
                "Horizontal alignment of plane-oriented text."));

            Params.RegisterInputParam(CreateTextParameter(
                "VerticalAlignment",
                "VA",
                "Vertical alignment of plane-oriented text."));
        }
        private static Param_Plane CreatePlaneParameter(
            string name,
            string nickname,
            string description,
            Plane defaultValue
        )
        {
            var parameter = new Param_Plane
            {
                Name = name,
                NickName = nickname,
                Description = description,
                Access = GH_ParamAccess.item
            };
            parameter.SetPersistentData(new GH_Plane(defaultValue));
            return parameter;
        }

        private static Param_Boolean CreateBooleanParameter(
            string name,
            string nickname,
            string description,
            bool defaultValue)
        {
            var parameter = new Param_Boolean
            {
                Name = name,
                NickName = nickname,
                Description = description,
                Access = GH_ParamAccess.item
            };
            parameter.SetPersistentData(new GH_Boolean(defaultValue));
            return parameter;
        }

        private static Param_String CreateTextParameter(
            string name,
            string nickname,
            string description)
        {
            return new Param_String
            {
                Name = name,
                NickName = nickname,
                Description = description,
                Access = GH_ParamAccess.item,
                Optional = true
            };
        }

        private void ScheduleAlignmentLists(GH_Document document)
        {
            if (document == null || _mode != RenderTagMode.OnPlane)
                return;

            document.ScheduleSolution(5, scheduledDocument =>
            {
                if (_mode != RenderTagMode.OnPlane || Params.Input.Count < 6)
                    return;

                CreateValueList(
                    scheduledDocument,
                    Params.Input[5],
                    HorizontalListName,
                    new[]
                    {
                        ("Auto", "Auto"),
                        ("Centre", "Center"),
                        ("Left", "Left"),
                        ("Right", "Right")
                    });

                CreateValueList(
                    scheduledDocument,
                    Params.Input[6],
                    VerticalListName,
                    new[]
                    {
                        ("Bottom", "Bottom"),
                        ("BottomOfBoundingBox", "BottomOfBoundingBox"),
                        ("BottomOfTop", "BottomOfTop"),
                        ("Middle", "Middle"),
                        ("MiddleOfBottom", "MiddleOfBottom"),
                        ("MiddleOfTop", "MiddleOfTop"),
                        ("Top", "Top")
                    });
            });
        }

        private void CreateValueList(
            GH_Document document,
            IGH_Param parameter,
            string listName,
            IEnumerable<(string Name, string Value)> items)
        {
            if (parameter == null ||
                parameter.SourceCount > 0 ||
                parameter.VolatileDataCount > 0)
            {
                return;
            }

            Attributes?.PerformLayout();

            var valueList = new GH_ValueList
            {
                Name = listName,
                NickName = listName
            };

            valueList.CreateAttributes();
            valueList.Attributes.Pivot = new PointF(
                parameter.Attributes.Pivot.X - 180f,
                parameter.Attributes.Pivot.Y);

            valueList.ListItems.Clear();
            valueList.ListItems.AddRange(items.Select(item =>
                new GH_ValueListItem(item.Name, $"\"{item.Value}\"")));

            document.AddObject(valueList, false);
            parameter.AddSource(valueList);
        }

        private void RemoveAutomaticAlignmentLists()
        {
            var document = OnPingDocument();
            if (document == null)
                return;

            foreach (var parameter in Params.Input.Skip(2).ToList())
            {
                foreach (var source in parameter.Sources.OfType<GH_ValueList>().ToList())
                {
                    if ((source.Name == HorizontalListName ||
                         source.Name == VerticalListName) &&
                        source.Recipients.Count <= 1)
                    {
                        document.RemoveObject(source, false);
                    }
                }
            }
        }

        private static bool TryParseHorizontalAlignment(
            string value,
            out TextHorizontalAlignment alignment)
        {
            if (string.Equals(value, "Centre", StringComparison.OrdinalIgnoreCase))
                value = "Center";

            return Enum.TryParse(value, true, out alignment);
        }

        private void UpdateMessage()
        {
            Message = _mode == RenderTagMode.OnWindow
                ? "Tag.OnWindow"
                : "Tag.OnPlane";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino;
using Rhino.DocObjects;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates Rhino layers under an optional parent and middle layer.
    /// </summary>
    public class GH_CreateLayers : GH_Component, IGH_VariableParameterComponent
    {
        public GH_CreateLayers()
            : base("Create Layers", "Layers", "Create Rhino layers under an optional parent and middle layer.", "Woodpecker", "Util")
        {
        }

        public override Guid ComponentGuid => new Guid("c70f67da-0c04-4f6c-95a2-8570bbf82d14");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index > 1 && index < Params.Input.Count;
        }

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return CanInsertParameter(side, index);
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index > 1 && index < Params.Input.Count - 1;
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return CanRemoveParameter(side, index);
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_String
            {
                Name = "Middle Layer",
                NickName = "Middle",
                Description = "Optional nested middle layer.",
                Access = GH_ParamAccess.item,
                Optional = true
            };
        }

        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return CreateParameter(side, index);
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return DestroyParameter(side, index);
        }

        public void VariableParameterMaintenance()
        {
            Params.Input[0].Name = "Create";
            Params.Input[0].NickName = "C";
            Params.Input[0].Description = "Create the layers when true.";
            Params.Input[0].Access = GH_ParamAccess.item;

            Params.Input[1].Name = "Parent Layer";
            Params.Input[1].NickName = "Parent";
            Params.Input[1].Description = "Existing parent layer full path. Leave empty to create layers at the root.";
            Params.Input[1].Access = GH_ParamAccess.item;
            Params.Input[1].Optional = true;

            for (int i = 2; i < Params.Input.Count - 1; i++)
            {
                var middleIndex = i - 1;
                Params.Input[i].Name = $"Middle Layer {middleIndex}";
                Params.Input[i].NickName = $"Middle{middleIndex}";
                Params.Input[i].Description = "Optional nested middle layer.";
                Params.Input[i].Access = GH_ParamAccess.item;
                Params.Input[i].Optional = true;
            }

            var last = Params.Input.Count - 1;
            Params.Input[last].Name = "Layer Names";
            Params.Input[last].NickName = "Layers";
            Params.Input[last].Description = "Layer names to create below the parent or middle layers.";
            Params.Input[last].Access = GH_ParamAccess.list;
            Params.Input[last].Optional = true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            VariableParameterMaintenance();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Create", "C", "Create the layers when true.", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Parent Layer", "Parent", "Existing parent layer full path. Leave empty to create layers at the root.", GH_ParamAccess.item, string.Empty);
            pManager.AddTextParameter("Middle Layer 1", "Middle1", "Optional nested middle layer.", GH_ParamAccess.item, string.Empty);
            pManager.AddTextParameter("Layer Names", "Layers", "Layer names to create below the parent or middle layer.", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Layer Paths", "Paths", "Full paths of created or targeted layers.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var create = false;
            var parentLayer = string.Empty;
            var middleLayers = new List<string>();
            var layerNames = new List<string>();

            DA.GetData("Create", ref create);
            DA.GetData("Parent Layer", ref parentLayer);

            var lastInput = Params.Input.Count - 1;
            for (int i = 2; i < lastInput; i++)
            {
                var middleLayer = string.Empty;
                DA.GetData(i, ref middleLayer);
                if (!string.IsNullOrWhiteSpace(middleLayer))
                    middleLayers.Add(middleLayer);
            }

            DA.GetDataList(lastInput, layerNames);

            var createdPaths = new List<string>();
            if (!create)
            {
                DA.SetDataList("Layer Paths", createdPaths);
                return;
            }

            if (layerNames == null || layerNames.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No layer names were provided.");
                DA.SetDataList("Layer Paths", createdPaths);
                return;
            }

            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "RhinoDoc.ActiveDoc cannot be found.");
                DA.SetDataList("Layer Paths", createdPaths);
                return;
            }

            var targetParentPath = parentLayer ?? string.Empty;
            foreach (var middleLayer in middleLayers)
            {
                AddChildLayer(doc, middleLayer, ref targetParentPath);
                createdPaths.Add(targetParentPath);
            }

            var fixedParentPath = targetParentPath;
            foreach (var layerName in layerNames)
            {
                if (string.IsNullOrWhiteSpace(layerName))
                    continue;

                var parentPath = fixedParentPath;
                AddChildLayer(doc, layerName, ref parentPath);
                createdPaths.Add(parentPath);
            }

            DA.SetDataList("Layer Paths", createdPaths);
        }

        private static int AddChildLayer(RhinoDoc doc, string name, ref string parentFullPath)
        {
            var newLayer = new Layer
            {
                Name = name,
                Color = Color.Black
            };

            var parentIdx = string.IsNullOrWhiteSpace(parentFullPath)
                ? -1
                : doc.Layers.FindByFullPath(parentFullPath, -1);

            if (parentIdx != -1)
                newLayer.ParentLayerId = doc.Layers[parentIdx].Id;

            if (parentIdx != -1)
                parentFullPath += "::" + name;
            else
                parentFullPath = name;

            return doc.Layers.Add(newLayer);
        }
    }
}

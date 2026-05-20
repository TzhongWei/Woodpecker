using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Woodpecker.Animation.Util.IO;
using System.CodeDom;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Creates or updates a named colour-code entry. A code name and colour values are encoded into the colour-code data structure for saving or downstream use. Inputs include CodeName and ColourList. Outputs include ColourCode.
    /// </summary>
    public class GH_CreateColourCode : GH_Component
    {
        public override GH_Exposure Exposure =>  GH_Exposure.tertiary;
        public GH_CreateColourCode()
          : base("Create ColourCodes", "CCC",
              "Create a new colour code",
              "Woodpecker", "ColourCode")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CodeName", "CN", "The name of the colour code to edit", GH_ParamAccess.item, "");
            pManager.AddColourParameter("ColourList", "Cs", "List of colours for the colour code", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var codeName = "";
            var colourList = new List<Color>();
            DA.GetData("CodeName", ref codeName);
            DA.GetDataList("ColourList", colourList);
            var cssList = new List<string>();
            foreach(var Col in colourList)
            {
                cssList.Add(ColourCodeUtil.ParseColourCss(Col));
            }
            var Code = new List<string>{codeName};
            Code.AddRange(cssList);
            DA.SetDataList("ColourCode", Code);
        }
        public override Guid ComponentGuid => new Guid("B2C3D4E5-6789-0ABC-DEF1-234567890ABC");
    }
}
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System;
using Woodpecker.Animation.Util.IO;
using System.Drawing;



namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Get colour from a colour code. Inputs include ColourCode. Outputs include CodeName and ColourList.
    /// </summary>
    public class GH_GetColour: GH_Component
    {
        public override GH_Exposure Exposure =>  GH_Exposure.tertiary;
        public GH_GetColour():base("Get Colour", "GC", "Get colour from a colour code", "Woodpecker", "ColourCode"){}
        public override Guid ComponentGuid => new Guid("17fcc77c-e793-4d4f-aff5-bf034505939a");
        protected override Bitmap Icon => Properties.Resources.GH_Get_Colour_Code;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("CodeName", "CN", "The name of the colour code to edit", GH_ParamAccess.tree);
            pManager.AddColourParameter("ColourList", "Cs", "List of colours for the colour code", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if(!DA.GetDataTree<GH_String>("ColourCode", out var CCTree) || CCTree == null)
                return;
            
            var CCName = new DataTree<string>();
            var CCColour = new DataTree<Color>();
            for(int i = 0; i < CCTree.Branches.Count; i++)
            {
                try
                {
                var Name = CCTree.Branches[i].First().Value;
                var Colour = CCTree.Branches[i].Skip(1).Select(x => ColourCodeUtil.ParseCssColour(x.Value)).ToList();
                CCName.Add(Name, new GH_Path(i));
                CCColour.AddRange(Colour, new GH_Path(i));
                }
                catch
                {
                    this.AddRuntimeMessage( GH_RuntimeMessageLevel.Error, $"Colour parsing failed at {string.Join(",", CCTree.Branches[i])}");
                }
            }

            
            DA.SetDataTree(0, CCName);
            DA.SetDataTree(1, CCColour);

        }
    }
}
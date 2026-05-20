using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using System.Drawing;
using Woodpecker.Animation.Geometry.Display;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// The vector display setting. Inputs include Length, ArrowheadSize, ArrowsRelativeSize, Width, and Color. Outputs include VectorDisplaySetting.
    /// </summary>
    public class GH_VectorDisplaySetting : GH_Component
    {
        public GH_VectorDisplaySetting():base("Vector Display Setting", "VDS", "The vector display setting", "Woodpecker", "Display"){}
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("540844db-42df-4ae8-8ed6-96c4eb21ea51");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length", "L", "Length of the vector display.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("ArrowheadSize", "AS", "If ArrowheadSize != 0.0 then the size (in screen pixels) of the arrow head will be equal to screenSize. Draws a single arrow object. An arrow consists of a Shaft and an Arrow head at the end of the shaft.", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("ArrowsRelativeSize", "ARS", "If relativeSize != 0.0 and screen size == 0.0 the size of the arrow head will be proportional to the arrow shaft length. Draws a single arrow object. An arrow consists of a Shaft and an Arrow head at the end of the shaft.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Width", "W", "Width of the vector display.", GH_ParamAccess.item, 1);
            pManager.AddColourParameter("Color", "C", "Color of the vector display.", GH_ParamAccess.item, Color.Black);
        }
        
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VectorDisplaySetting", "VDS", "The vector display setting to be used for drawing vectors in the viewport.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var length = 0.0;
            var arrowheadSize = 0.0;
            var width = 1;
            var color = Color.Black;
            // var displaytypeIndex = 1;

            DA.GetData("Length", ref length);
            DA.GetData("ArrowheadSize", ref arrowheadSize);
            DA.GetData("Width", ref width);
            DA.GetData("Color", ref color);
            // DA.GetData("DisplayType", ref displaytypeIndex);

            // VectorDisplayType vectorDisplayType = VectorDisplayType.DirectionArrow;
            // switch (displaytypeIndex)
            // {
            //     case 0:
            //         vectorDisplayType = VectorDisplayType.DirectionArrow;
            //         break;
            //     case 1:
            //         vectorDisplayType = VectorDisplayType.HollowArrow;
            //         break;
            //     case 2:
            //         vectorDisplayType = VectorDisplayType.SolidArrow;
            //         break;
            //     default:
            //         vectorDisplayType = VectorDisplayType.DirectionArrow;
            //         break;
            // }
            var vectorDisplaySetting = new VectorDisplaySetting(length, arrowheadSize, width, color);
            // vectorDisplaySetting.DisplayType = vectorDisplayType;
            DA.SetData("VectorDisplaySetting", vectorDisplaySetting);
        }
    }

}
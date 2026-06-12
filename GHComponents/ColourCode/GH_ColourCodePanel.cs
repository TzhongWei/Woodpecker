using Grasshopper.Kernel;
using Grasshopper;
using System;
using System.Drawing;
using Woodpecker.Animation.Util.IO;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Woodpecker.Animation.GHComponents.CustomGHComponents;
using System.Collections.Generic;
using System.Linq;

namespace Woodpecker.Animation.GHComponents
{
    /// <summary>
    /// Display the colour code in a panel. Inputs include ColourCode.
    /// </summary>
    public class GH_ColourCodePanel : GH_Component
    {
        public override GH_Exposure Exposure =>  GH_Exposure.primary;
        public Dictionary<string, List<Color>> ColourCodeDic { private set; get; } = new Dictionary<string, List<Color>>();
        //public Color DisplayColour { get; set; } = Color.Gray;
        //public string DisplayName { get; set; } = "Colour Code";
        //public void SetDisplayColourByCss(string displayColour)
        //{
        //    this.DisplayColour = ColourCode.ParseCssColor(displayColour);
        //}
        public GH_ColourCodePanel()
          : base("ColourCode Panel", "CCP",
              "Display the colour code in a panel",
              "Woodpecker", "ColourCode")
        {
        }
        public override void CreateAttributes()
        {
            m_attributes = new ColourDisplayAttributes(this, this.ColourCodeDic);
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ColourCode", "CC", "Encoded colour code", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // No outputs
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree<GH_String>(0, out var CC) || CC == null || CC.Branches.Count == 0)
            {
                this.ColourCodeDic = new Dictionary<string, List<Color>>();
                UpdateAttributes();
                return;
            }

            var CCDic = new Dictionary<string, List<string>>();
            for (int i = 0; i < CC.Branches.Count; i++)
            {
                var branch = CC.Branches[i];
                if (branch == null || branch.Count == 0)
                    continue;

                CCDic[branch[0].Value] = branch.Skip(1).Select(x => x.Value).ToList();
            }
            this.ColourCodeDic = ColourCodeUtil.StringToColourDictionary(CCDic);

            UpdateAttributes();
        }

        private void UpdateAttributes()
        {
            if (!(m_attributes is ColourDisplayAttributes attributes) ||
                !attributes.UpdateColourCode(ColourCodeDic))
                return;

            attributes.ExpireLayout();
            OnDisplayExpired(false);
        }

        public override Guid ComponentGuid => new Guid("A1B2C3D4-5678-90AB-CDEF-1234567890AB");
        protected override Bitmap Icon => Properties.Resources.GH_Colour_Panel;
    }
}

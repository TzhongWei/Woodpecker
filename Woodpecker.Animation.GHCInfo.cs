using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Woodpecker.Animation
{
    public class WoodpeckerAnimationGHCInfo : GH_AssemblyInfo
    {
        public override string Name => "Woodpecker Animation";
        public override Bitmap Icon => Properties.Resources.Woodpecker_Animation_Icon;
        public override string Description => "Animation tools developed by IBOIS.";
        public override Guid Id => new Guid("7eb21059-d3ca-4804-846f-f9d2e1fe3fba");
        public override string AuthorName => "IBOIS";
        public override string AuthorContact => "tsungwei.cheng@epfl.ch";
    }
}

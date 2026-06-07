using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Util.IO;

namespace Woodpecker.Animation
{
  public class IBOIS_AnimationInfo : GH_AssemblyInfo, VersionControl
  {
    public override string Name => "IBOIS.Animation Info";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => Properties.Resources.Woodpecker_Animation_Icon;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "";

    public override Guid Id => new Guid("c475b049-0e36-4580-9ef2-146d9090f364");

    //Return a string identifying you or your company.
    public override string AuthorName => "TsungWei Cheng Mike";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "tsungwei.cheng@epfl.ch";

    //Return a string representing the version.  This returns the same version as the assembly.
    public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    public override string Version => "v1.1.0"; 
  }
}
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.CodeManager;
using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.GHComponents
{
    public abstract class GH_TagChannel_Abstract : GH_Component, ISingletonDocumentComponent
    {
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        /// <summary>
        /// Check the tag for input or output, 
        /// for all input, the tag should be unique, true -> is unique, false -> is duplicated ||
        /// for all ouput, the tag should be exist,  true -> is exis, false -> isn't defined 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsPrimaryInstance();
        public string SingletonTag { get; protected set; }
        public TagChannel<IGH_Goo> tagChannel;
        public GH_TagChannel_Abstract(string Name, string NickName, string Description) : base(Name, NickName, Description, "Woodpecker", "CodeManager") { }
        public abstract RemoteType ChannelType { get; }
        protected static string ChannelKey(IGH_Param param, int fallbackIndex)
        {
            if (param == null || string.IsNullOrWhiteSpace(param.NickName) || param.NickName == "{ }")
                return fallbackIndex.ToString();
            return param.NickName.Trim();
        }
        protected override void AfterSolveInstance()
        {
            if (string.IsNullOrWhiteSpace(SingletonTag)) return;

            if (!IsPrimaryInstance())
            {
                if (this.ChannelType == RemoteType.Input)
                {
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, $"The tag {SingletonTag} is exist, which cannot be named again");
                }
                if (this.ChannelType == RemoteType.Output)
                {
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, $"The tag {SingletonTag} is not defined, please try different tags");
                }
            }
        }

    }
}

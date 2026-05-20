using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public abstract class GH_TimeSlotChannel_Abstract : GH_TagChannel_Abstract
    {
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public GH_TimeSlotChannel_Abstract(string Name, string NickName, string Description, string SubCategory) : base(Name, NickName, Description)
        {
            this.SubCategory = SubCategory;
        }
        private TimeSlotTagChannel _tagChannel;
        public TimeSlotTagChannel GetTimeSlotTagChannel() => _tagChannel;
        public override TagChannel<IGH_Goo> tagChannel { get
        {
            return _tagChannel;} 
        set
            {
                _tagChannel = value;
            }
        }
    }
}
using System.CodeDom;
using System.Linq;
using System.Runtime.InteropServices;
using Grasshopper.Kernel.Types;

namespace Woodpecker.Animation.CodeManager
{
    public class TimeSlotTagChannel : ITagChannel<double>
    {
        public string TagName { get; private set; }

        public double Value { get; private set; }

        public TimeSlotTagChannel(string TagName)
        {
            this.TagName = TagName;
        }
        private bool _hasValue;
        public bool HasValidChannel() => _hasValue;
        public void SetValue(double value)
        {
            Value = value;
            _hasValue = !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static implicit operator TimeSlotTagChannel(TagChannel<IGH_Goo> tagChannel)
        {
            var tagChannelValue = tagChannel.Value;
            var resultValue = -1.0;
            foreach (var value in tagChannelValue)
            {
                if (value.Value.AllData().Any(x => x.ScriptVariable().GetType() == typeof(double)))
                {
                    resultValue = (double)value.Value.AllData().Where(x => x.ScriptVariable().GetType() == typeof(double)).FirstOrDefault().ScriptVariable();
                }
            }
            var newTag = new TimeSlotTagChannel(tagChannel.TagName);
            newTag.Value = resultValue;
            return newTag;
        }
        public static implicit operator TagChannel<IGH_Goo>(TimeSlotTagChannel tagChannel)
        {
            var Otagchannel = new TagChannel<IGH_Goo>(tagChannel.TagName);
            Otagchannel[tagChannel.TagName] = new Grasshopper.DataTree<IGH_Goo>();
            var gh_number = new GH_Number(tagChannel.Value);
            Otagchannel[tagChannel.TagName].Add(gh_number);
            return Otagchannel;
        }
    }
}
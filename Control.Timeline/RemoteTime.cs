using System;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.Control.Timeline
{
    [Obsolete]
    public class RemoteTime : ITagChannel<double>
    {
        public double Value { get; set; }
        public string TagName { get; }
        public bool HasValidChannel() => true;
        public RemoteTime(string tag)
        {
            Value = 0;
            this.TagName = tag;
        }
        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            if (obj is RemoteTime objTime)
            {
                return objTime.TagName == this.TagName && objTime.Value == this.Value;
            }
            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (TagName?.GetHashCode() ?? 0);
                hash = hash * 23 + Value.GetHashCode();
                return hash;
            }
        }
    }
}
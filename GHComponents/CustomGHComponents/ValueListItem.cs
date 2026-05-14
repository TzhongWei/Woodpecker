using System.Drawing;
using Grasshopper.Kernel.Special;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    public abstract class ValueListItem
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public RectangleF BoxLeft;
        public RectangleF BoxName;
        protected ValueListItem() : base()
        {

        }
        protected ValueListItem(string name, string expression)
        {
            this.Name = name;
            this.Expression = expression;
        }
        public abstract ValueListItem Clone();
        internal virtual void SetCheckListBounds(RectangleF bounds)
        {
            BoxLeft = RectangleF.Empty;
            BoxName = bounds;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is ValueListItem other)
            {
                return string.Equals(Name, other.Name, System.StringComparison.Ordinal) &&
                       string.Equals(Expression, other.Expression, System.StringComparison.Ordinal) &&
                       Selected == other.Selected;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (Expression == null ? 0 : Expression.GetHashCode());
                hash = hash * 23 + Selected.GetHashCode();
                return hash;
            }
        }
    }
}

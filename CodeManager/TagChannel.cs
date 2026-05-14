using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Woodpecker.Animation.CodeManager
{
    public enum RemoteType
    {
        Input,
        Output
    }
    public class TagChannel<T>: ITagChannel<Dictionary<string, DataTree<T>>>
    {
        public TagChannel(string Tag)
        {
            this.TagName = Tag;
            Value = new Dictionary<string, DataTree<T>>();
        }
        public TagChannel(string Tag, T Data) : this(Tag)
        {
            
            Value = new Dictionary<string, DataTree<T>>();
            Value["A"] = new DataTree<T>();
            Value["A"].Add(Data);
        }
        public TagChannel(string Tag, IEnumerable<T> Data):this(Tag)
        {
            Value = new Dictionary<string, DataTree<T>>();
            Value["A"] = new DataTree<T>();
            Value["A"].AddRange(Data);
        }
        public TagChannel(string Tag, DataTree<T> Data) : this(Tag)
        {
            Value = new Dictionary<string, DataTree<T>>();
            Value["A"] = Data;
        }
        public DataTree<T> this[string key]
        {
            get => Value[key];
            set => Value[key] = value;
        }
        public string TagName {get; private set;}
        public bool HasValidChannel() => Value != null;
        public Dictionary<string, DataTree<T>> Value {get; private set;}
        public int Count => Value.Count;
        public IEnumerable<string> Keys => Value.Keys;
        public bool TryGetValue(string key, out DataTree<T> dataTree) => Value.TryGetValue(key, out dataTree);
        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            if (obj is TagChannel<T> objTime)
            {
                return objTime.TagName == this.TagName && objTime.Value.Equals(this.Value);
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

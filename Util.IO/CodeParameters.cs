using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Rhino.UI.Controls;

namespace Woodpecker.Animation.Util.IO
{
    public abstract class CodeParameters<T>
    {
        protected Dictionary<string, List<T>> _values;
        public bool IsValid => _values != null && _values.Count != 0;
        public List<string> Labels => _values?.Keys.ToList() ?? new List<string>();
        public Dictionary<string, List<T>> Values => _values;
        public int Count => _values?.Count ?? 0;
        public CodeParameters()
        {
            _values = new Dictionary<string, List<T>>();
        }
        public CodeParameters(Dictionary<string, List<T>> values)
        {
            _values = values ?? new Dictionary<string, List<T>>();
        }
        public List<T> this[string key]
        {
            get
            {
                if (_values != null && _values.TryGetValue(key, out var values))
                    return values;
                else
                    return new List<T>();
            }
            set
            {
                _values[key] = value;
            }
        }
        public abstract DataTree<string> To_GH_DataTree();
    }
}
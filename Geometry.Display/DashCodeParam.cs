using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.Geometry.Display
{
    public class DashCodeParam : IList<DashType>, ITagChannel<Dictionary<string, DashType>>
    {
        private readonly Dictionary<string, DashType> _dashTypes;
        private readonly List<string> _order;

        public DashCodeParam(string CodeName)
        {
            this.TagName = CodeName;
            _dashTypes = new Dictionary<string, DashType>();
            _order = new List<string>();
        }
        public DashCodeParam(string CodeName, List<DashType> types) : this(CodeName)
        {
            _dashTypes = new Dictionary<string, DashType>();
            foreach(var type in types)
            {
                _dashTypes.Add(type.Name, type);
            }
            _order = _dashTypes.Keys.ToList();
        }
        public DashCodeParam(string CodeName, Dictionary<string, DashType> dashTypes) : this(CodeName)
        {
            _dashTypes = dashTypes != null
                ? new Dictionary<string, DashType>(dashTypes)
                : new Dictionary<string, DashType>();
            _order = _dashTypes.Keys.ToList();
        }

        public DashType this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Dash type name cannot be null or empty.", nameof(name));

                if (!_dashTypes.TryGetValue(name, out var dashType))
                    throw new KeyNotFoundException($"Dash type '{name}' was not found.");

                return dashType;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Dash type name cannot be null or empty.", nameof(name));
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (!_dashTypes.ContainsKey(name))
                    _order.Add(name);

                value.Name = name;
                _dashTypes[name] = value;
            }
        }

        public DashType this[int index]
        {
            get
            {
                var key = _order[index];
                return _dashTypes[key];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                var oldKey = _order[index];
                var newKey = value.Name;

                if (string.IsNullOrWhiteSpace(newKey))
                    throw new ArgumentException("Dash type name cannot be null or empty.", nameof(value));

                if (oldKey != newKey && _dashTypes.ContainsKey(newKey))
                    throw new ArgumentException($"Dash type '{newKey}' already exists.", nameof(value));

                _dashTypes.Remove(oldKey);
                _order[index] = newKey;
                _dashTypes[newKey] = value;
            }
        }

        public int Count => _order.Count;

        public bool IsReadOnly => false;

        public IEnumerable<string> Names => _order;

        public Dictionary<string, DashType> Values => _order.ToDictionary(name => name, name => _dashTypes[name]);

        public string TagName {get; private set;}

        public Dictionary<string, DashType> Value => this._dashTypes;
        public void Set_ScalebyScreen(bool ScalebyScreen)
        {
            foreach(var item in _dashTypes)
            {
                _dashTypes[item.Key].ScalebyScreen = ScalebyScreen;
            }
        }
        public void Add(DashType item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Dash type name cannot be null or empty.", nameof(item));

            if (!_dashTypes.ContainsKey(item.Name))
                _order.Add(item.Name);

            _dashTypes[item.Name] = item;
        }

        public bool TryGetDashType(string name, out DashType dashType)
        {
            dashType = null;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return _dashTypes.TryGetValue(name, out dashType);
        }

        public bool ContainsName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && _dashTypes.ContainsKey(name);
        }

        public void Clear()
        {
            _dashTypes.Clear();
            _order.Clear();
        }

        public bool Contains(DashType item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name))
                return false;

            return _dashTypes.TryGetValue(item.Name, out var existing) && ReferenceEquals(existing, item);
        }

        public void CopyTo(DashType[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            foreach (var dashType in this)
            {
                array[arrayIndex++] = dashType;
            }
        }

        public IEnumerator<DashType> GetEnumerator()
        {
            foreach (var name in _order)
            {
                yield return _dashTypes[name];
            }
        }

        public int IndexOf(DashType item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name))
                return -1;

            return _order.IndexOf(item.Name);
        }

        public void Insert(int index, DashType item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Dash type name cannot be null or empty.", nameof(item));
            if (_dashTypes.ContainsKey(item.Name))
                throw new ArgumentException($"Dash type '{item.Name}' already exists.", nameof(item));

            _order.Insert(index, item.Name);
            _dashTypes[item.Name] = item;
        }

        public bool Remove(DashType item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name))
                return false;

            if (!_dashTypes.TryGetValue(item.Name, out var existing) || !ReferenceEquals(existing, item))
                return false;

            _dashTypes.Remove(item.Name);
            _order.Remove(item.Name);
            return true;
        }

        public bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!_dashTypes.Remove(name))
                return false;

            _order.Remove(name);
            return true;
        }

        public void RemoveAt(int index)
        {
            var name = _order[index];
            _order.RemoveAt(index);
            _dashTypes.Remove(name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool HasValidChannel() => this.Value != null;
    }
}
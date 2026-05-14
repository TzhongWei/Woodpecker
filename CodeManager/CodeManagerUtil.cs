using System;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System.Collections.Generic;
using System.Linq;

namespace Woodpecker.Animation.CodeManager
{
    public enum ManageType
    {
        Group,
        Component,
    }
    public class CodeManagerUtil
    {
        private bool? _preEnable = null;
        private bool? _preDisplay = null;
        private readonly string _manageName;
        private readonly GH_Document _doc;
        private ManageType _manageType;
        public CodeManagerUtil(GH_Document doc, string Name, ManageType manageType = ManageType.Group)
        {
            if (String.IsNullOrWhiteSpace(Name) || doc == null)
            {
                throw new Exception("Name or Grasshopper Document cannot be null");
            }
            _manageType = manageType;
            _manageName = Name;
            _doc = doc;
        }
        public bool EnableToggle(bool Enable)
        {
            if (_manageType == ManageType.Group)
                return Group_EnableToggle(Enable);
            else if (_manageType == ManageType.Component)
                return Component_EnableToggle(Enable);
            else
                return false;
        }
        public bool DisplayToggle(bool Enable)
        {
            if (_manageType == ManageType.Group)
                return Group_DisplayToggle(Enable);
            else if (_manageType == ManageType.Component)
                return Component_DisplayToggle(Enable);
            else
                return false;
        }
        private bool Component_EnableToggle(bool Enable)
        {
            if (_preEnable.HasValue && _preEnable.Value == Enable)
            {
                return true;
            }
            _preEnable = Enable;
            try
            {
                var components = _doc.Objects.OfType<GH_Component>()
                    .Where(g => g.Name == _manageName || g.NickName == _manageName)
                    .ToList();
                var changed = new List<IGH_DocumentObject>();

                foreach (var targetComponent in components)
                {
                    var obj = _doc.FindObject(targetComponent.InstanceGuid, true);
                    if (obj == null) continue;

                    if (!(obj is IGH_ActiveObject actObj)) continue;

                    var newLocked = !Enable;
                    if (actObj.Locked != newLocked)
                    {
                        changed.Add(obj);
                    }
                }

                if (changed.Count == 0)
                    return false;

                _doc.ScheduleSolution(1, d =>
                {
                    foreach (var target in changed)
                    {
                        var obj = d.FindObject(target.InstanceGuid, true);
                        if (obj == null) continue;
                        if (!(obj is IGH_ActiveObject actObj)) continue;

                        actObj.Locked = !Enable;
                        if (Enable)
                        {
                            obj.ExpireSolution(false);
                        }
                    }

                    d.NewSolution(false);
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool Group_EnableToggle(bool Enable)
        {
            if (_preEnable.HasValue && _preEnable.Value == Enable)
            {
                return true;
            }
            _preEnable = Enable;
            try
            {
                var groups = _doc.Objects.OfType<GH_Group>().Where(g => g.NickName == _manageName).ToList();
                var changed = new List<IGH_DocumentObject>();

                foreach (var targetGroup in groups)
                {
                    foreach (var id in targetGroup.ObjectIDs)
                    {
                        var obj = _doc.FindObject(id, true);
                        if (obj == null) continue;

                        if (!(obj is IGH_ActiveObject actObj)) continue;

                        var newLocked = !Enable;
                        if (actObj.Locked != newLocked)
                        {
                            changed.Add(obj);
                        }
                    }
                }

                if (changed.Count == 0)
                    return false;

                _doc.ScheduleSolution(1, d =>
                {
                    foreach (var target in changed)
                    {
                        var obj = d.FindObject(target.InstanceGuid, true);
                        if (obj == null) continue;
                        if (!(obj is IGH_ActiveObject actObj)) continue;

                        actObj.Locked = !Enable;
                        if (Enable)
                        {
                            obj.ExpireSolution(false);
                        }
                    }

                    d.NewSolution(false);
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool Component_DisplayToggle(bool Enable)
        {
            if (_preDisplay.HasValue && _preDisplay.Value == Enable)
            {
                return true;
            }
            _preDisplay = Enable;

            try
            {
                var components = _doc.Objects.OfType<GH_Component>()
                    .Where(g => g.Name == _manageName || g.NickName == _manageName)
                    .ToList();
                var changed = new List<IGH_DocumentObject>();

                foreach (var targetComponent in components)
                {
                    var obj = _doc.FindObject(targetComponent.InstanceGuid, true);
                    if (obj == null) continue;
                    if (!(obj is IGH_PreviewObject previewObj)) continue;

                    if (previewObj.Hidden == Enable)
                    {
                        changed.Add(obj);
                    }
                }

                if (changed.Count == 0)
                    return false;

                _doc.ScheduleSolution(1, d =>
                {
                    foreach (var target in changed)
                    {
                        var obj = d.FindObject(target.InstanceGuid, true);
                        if (obj == null) continue;
                        if (!(obj is IGH_PreviewObject previewObj)) continue;

                        previewObj.Hidden = !Enable;
                    }

                    d.ScheduleSolution(1, _ => { });
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool Group_DisplayToggle(bool Enable)
        {
            if (_preDisplay.HasValue && _preDisplay.Value == Enable)
            {
                return true;
            }
            _preDisplay = Enable;
            try
            {
                var groups = _doc.Objects.OfType<GH_Group>().Where(g => g.NickName == _manageName).ToList();
                var changed = new List<IGH_DocumentObject>();

                foreach (var targetGroup in groups)
                {
                    foreach (var id in targetGroup.ObjectIDs)
                    {
                        var obj = _doc.FindObject(id, true);
                        if (obj == null) continue;

                        if (!(obj is IGH_PreviewObject previewObj)) continue;

                        if (previewObj.Hidden == Enable)
                        {
                            changed.Add(obj);
                        }
                    }
                }

                if (changed.Count == 0)
                    return false;

                _doc.ScheduleSolution(1, d =>
                {
                    foreach (var target in changed)
                    {
                        var obj = d.FindObject(target.InstanceGuid, true);
                        if (obj == null) continue;

                        if (!(obj is IGH_PreviewObject previewObj)) continue;

                        previewObj.Hidden = !Enable;
                    }

                    d.ScheduleSolution(1, _ => { });
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is CodeManagerUtil util)
            {
                return this._manageName == util._manageName
    && this._manageType == util._manageType
    && ReferenceEquals(this._doc, util._doc);
            }
            return false;
        }
        public static bool operator ==(CodeManagerUtil util1, CodeManagerUtil util2)
        {
            if (ReferenceEquals(util1, util2)) return true;
            if (util1 is null || util2 is null) return false;
            return util1.Equals(util2);
        }
        public static bool operator !=(CodeManagerUtil util1, CodeManagerUtil util2)
        {
            return !(util1 == util2);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (_manageName?.GetHashCode() ?? 0);
                hash = hash * 23 + _manageType.GetHashCode();
                hash = hash * 23 + (_doc?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
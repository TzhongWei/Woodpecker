using System.Linq.Expressions;
using Grasshopper.Kernel;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    /// <summary>
    /// Code Manager Abstract component.
    /// </summary>
    public abstract class GH_CodeManagerAbstract : GH_Component
    {
        protected CodeManagerUtil _codeManagerUtil = null;
        protected bool _result;
        protected string _reportMessage;
        protected GH_Document _doc;
        public GH_CodeManagerAbstract(string Name, string NickName, string Description) :
        base(Name, NickName, Description, "Woodpecker", "CodeManager")
        {
        }
        protected void setdoc()
        {
            _doc = this.OnPingDocument();
            if (_doc == null) throw new System.Exception("GH_document is null");
        }
    }
}
using System.Linq;
using System.Linq.Expressions;
using Grasshopper.Kernel;

namespace Woodpecker.Animation.CodeManager
{
    public static class RefleshGHDocument
    {
        public static bool RefleshComponents(GH_Document doc, string UpdateTag)
        {
            if (doc == null || string.IsNullOrWhiteSpace(UpdateTag))
                return false;

            var targets = doc.Objects
                .OfType<GH_Component>()
                .Where(x => x is IUpdateDependent dep && dep.UpdateTag == UpdateTag)
                .ToList();

            if (targets.Count == 0)
                return false;

            foreach (var component in targets)
            {
                component.ExpireSolution(false);
            }

            doc.ScheduleSolution(1);

            return true;

        }
    }
}
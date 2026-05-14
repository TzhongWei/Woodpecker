using Grasshopper.Kernel;

namespace Woodpecker.Animation.CodeManager
{
    public interface IUpdateDependent
    {
        string UpdateTag { get; }
    }
}
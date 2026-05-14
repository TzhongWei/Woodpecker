using Woodpecker.Animation.Control.Timeline;

namespace Woodpecker.Animation.CodeManager
{
    public interface ITagChannel<T>
    {
        string TagName { get; }
        bool HasValidChannel();
        T Value{ get; }
    }
}

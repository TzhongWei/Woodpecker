namespace Woodpecker.Animation.CodeManager
{
    public interface IHasMultipleActiveInstanceDocumentComponent
    {
        string MultiTag {get;}
        bool HasMultipleActiveInstance();
    }
}
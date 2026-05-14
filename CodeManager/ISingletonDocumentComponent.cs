public interface ISingletonDocumentComponent
{
    string SingletonTag{get;}
    bool IsPrimaryInstance();
}
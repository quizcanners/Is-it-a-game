namespace Dungeons_and_Dragons
{
    public interface IConceptValueProvider
    {
        bool TryGetConcept<T>(out T value) where T : System.IComparable;
    }
}

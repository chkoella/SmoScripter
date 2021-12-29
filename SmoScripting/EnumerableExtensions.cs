namespace SmoScripting;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> AddAfterEachElement<TSource>(this IEnumerable<TSource> source, TSource additionalElement)
    {
        foreach (var sourceElement in source)
        {
            yield return sourceElement;
            yield return additionalElement;
        }
    }
}

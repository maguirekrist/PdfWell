namespace PDFParser.Parser.Utils;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Flatten<T>(
        this IEnumerable<T> source,
        Func<T, IEnumerable<T>> childSelector)
    {
        foreach (var item in source)
        {
            yield return item;

            foreach (var child in childSelector(item).Flatten(childSelector))
            {
                yield return child;
            }
        }
    }
}
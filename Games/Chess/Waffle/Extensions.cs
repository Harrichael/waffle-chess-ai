using System;
using System.Linq;
using System.Collections.Generic;


static class Extensions
{
    public static T MinByValue<T, K>(this IEnumerable<T> source, Func<T, K> selector)
    {
        var comparer = Comparer<K>.Default;

        var enumerator = source.GetEnumerator();
        enumerator.MoveNext();

        var min = enumerator.Current;
        var minV = selector(min);

        while (enumerator.MoveNext())
        {
            var s = enumerator.Current;
            var v = selector(s);
            if (comparer.Compare(v, minV) < 0)
            {
                min = s;
                minV = v;
            }
        }
        return min;
    }

    public static T MaxByValue<T, K>(this IEnumerable<T> source, Func<T, K> selector)
    {
        var comparer = Comparer<K>.Default;

        var enumerator = source.GetEnumerator();
        enumerator.MoveNext();

        var max = enumerator.Current;
        var maxV = selector(max);

        while (enumerator.MoveNext())
        {
            var s = enumerator.Current;
            var v = selector(s);
            if (comparer.Compare(v, maxV) > 0)
            {
                max = s;
                maxV = v;
            }
        }
        return max;
    }
}
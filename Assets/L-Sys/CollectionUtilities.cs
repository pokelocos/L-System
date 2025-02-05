using System.Collections.Generic;
using System;

public static class CollectionUtilities
{
    public static T RandomRullete<T>(this IEnumerable<T> collection, Func<T, float> aa)
    {
        var max = 0f;

        foreach (var item in collection)
            max += aa(item);

        var random = UnityEngine.Random.Range(0f, max);

        var acumulated = 0f;
        foreach (var item in collection)
        {
            acumulated += aa(item);
            if (random <= acumulated)
            {
                return item;
            }
        }

        return default(T);
    }
}

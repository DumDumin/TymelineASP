using System;
using System.Collections.Generic;

public static class CollectionExtension
{
    private static Random rng = new Random();

    public static T RandomElement<T>(this IList<T> list)
    {
        return list[rng.Next(list.Count)];
    }

    public static T RandomElement<T>(this T[] array)
    {
        return array[rng.Next(array.Length)];
    }
}
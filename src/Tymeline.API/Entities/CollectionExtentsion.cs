using System;
using System.Collections.Generic;
using System.Linq;

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


    public static IList<T> Clone<T>(this IList<T> listToClone) where T: ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    public static List<T> RandomElements<T>(this List<T> list, int count){
        if(count>list.Count){
            return list;
        }
        else{
            var copyList = list.ToList();
            var ret = new List<T>();
            for (int i = 0 ; i < count; i++)
            {
                var element = copyList.RandomElement();
                ret.Add(element);
                copyList.Remove(element);

            }
            return ret;
        }
    }
}
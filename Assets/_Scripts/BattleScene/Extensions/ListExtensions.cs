using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static T Draw<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            int r = Random.Range(0, list.Count);
            T item = list[r];
            list.RemoveAt(r);
            return item;
        }
        return default(T);
    }
}

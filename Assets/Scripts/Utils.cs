using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static T[] Slice<T>(T[] samples, int min, int max)
    {
        List<T> result = new List<T>();
        for (int i = min; i < max; i++)
        {
            result.Add(samples[i]);
        }

        return result.ToArray();
    }
}

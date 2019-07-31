using System;
using System.Collections.Generic;
using System.Text;

namespace FastDFS.Client
{
    public static class ArrayExtensions
    {
        public static void Fill<T>(this T[] array, T payload)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = payload;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EtwStream
{
    internal static class EnumerableExtensions
    {
        public static void FastForEach<T>(this IList<T> source, Action<T> action)
        {
            var l = source as List<T>;
            if (l != null)
            {
                l.ForEach(action);
                return;
            }

            var a = source as T[];
            if (a != null)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    action(a[i]);
                }
                return;
            }

            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}

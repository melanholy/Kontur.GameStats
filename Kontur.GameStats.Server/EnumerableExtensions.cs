using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server
{
    public static class EnumerableExtensions
    {
        public static bool Equals<T>(this IEnumerable<T> one, IEnumerable<T> another,
            Func<T, IComparable> orderSelector)
        {
            if (another == null)
                return false;

            one = one.ToList();
            another = another.ToList();
            if (one.Count() != another.Count())
                return false;

            return one
                .OrderBy(orderSelector)
                .SequenceEqual(another.OrderBy(orderSelector));
        }
    }
}
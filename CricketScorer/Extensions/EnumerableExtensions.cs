using System;
using System.Collections.Generic;
using System.Linq;

namespace CricketScorer.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Will return the element at <param name="index" /> or default.
        /// </summary>
        /// <typeparam name="T">The type defined by the enum contents.</typeparam>
        /// <param name="enumerable">The enumerable to operate on &amp; retrieve the item from.</param>
        /// <param name="index">The integer index of the element to return.</param>
        /// <returns>An element of the enum at position index or default.</returns>
        public static T At<T>(this IEnumerable<T> enumerable, int index)
        {
            try
            {
                return enumerable.ElementAt(index);
            }
            catch (ArgumentOutOfRangeException)
            {
                return default;
            }
        }
    }
}

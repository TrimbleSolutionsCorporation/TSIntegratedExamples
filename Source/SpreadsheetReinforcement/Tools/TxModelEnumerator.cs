namespace SpreadsheetReinforcement.Tools
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class TxModelEnumerator
    {
        /// <summary>
        /// Gets the list of the objects to which the <paramref name="enumerator" /> is pointing.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="enumerator">The target enumerator.</param>
        /// <returns>The list of the objects to which the <paramref name="enumerator" /> is pointing.</returns>
        [DebuggerStepThrough]
        public static List<TSource> ToList<TSource>(this IEnumerator enumerator)
        {
            var result = new List<TSource>();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current is TSource source)
                {
                    result.Add(source);
                }
            }

            return result;
        }
    }
}
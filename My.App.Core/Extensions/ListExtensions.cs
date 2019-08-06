using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace My.App.Core
{
    public static class ListExtensions
    {
        public static List<TSource> Clone<TSource>(this List<TSource> source)
        {
            if (source != null)
            {
                var jsonStr = JsonHelper.Serialize(source);
                return JsonHelper.Deserialize<List<TSource>>(jsonStr);
            }
            return default(List<TSource>);
        }
    }
}

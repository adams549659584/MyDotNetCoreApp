using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace My.App.Core
{
    public static class StringExtensions
    {
        public static int ToInt(this string str, int defaultVal = 0)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultVal;
            }
            int val = defaultVal;
            int.TryParse(str, out val);
            return val;
        }
    }
}

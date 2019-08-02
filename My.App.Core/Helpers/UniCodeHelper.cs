using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace My.App.Core
{
    public class UnicodeHelper
    {
        /// <summary>
        /// 转中文
        /// </summary>
        /// <param name="str">unicode字符串</param>
        /// <returns></returns>
        public static string ToChinese(string str)
        {
            return Regex.Unescape(str);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using RestSharp;

namespace My.App.Core
{
    public static class RestSharpHelper
    {
        static RestSharpHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// 根据<see cref="IRestResponse.ContentEncoding"/>或<see cref="IRestResponse.ContentType"/>设置<see cref="IRestResponse.Content"/>
        /// </summary>
        /// <param name="response">Rest响应实体</param>
        public static void SetResponseEncoding(this IRestResponse response)
        {
            var encoding = response.ContentEncoding;
            if (string.IsNullOrWhiteSpace(encoding) && !string.IsNullOrWhiteSpace(response.ContentType))
            {
                var tmp = response.ContentType.Split(';').Select(s => s.Split('='));
                var arr = tmp.LastOrDefault(t => t.Length == 2 && t[0].Trim().ToLower() == "charset");
                if (arr != null)
                {
                    encoding = arr[1].Trim();
                }
            }
            if (!string.IsNullOrWhiteSpace(encoding))
            {
                response.SetResponseEncoding(encoding);
            }
        }
        /// <summary>
        /// 根据Encoding设置<see cref="IRestResponse.Content"/>
        /// </summary>
        /// <param name="response">Rest响应实体</param>
        /// <param name="encoding">响应内容编码方式</param>
        public static void SetResponseEncoding(this IRestResponse response, string encoding)
        {
            if (!string.IsNullOrWhiteSpace(encoding))
            {
                response.SetResponseEncoding(Encoding.GetEncoding(encoding));
            }
        }
        /// <summary>
        /// 根据Encoding设置<see cref="IRestResponse.Content"/>
        /// </summary>
        /// <param name="response">Rest响应实体</param>
        /// <param name="encoding">响应内容编码方式</param>
        public static void SetResponseEncoding(this IRestResponse response, Encoding encoding)
        {
            if (encoding != null)
            {
                response.ContentEncoding = encoding.WebName;
                response.Content = encoding.GetString(response.RawBytes);
            }
        }
    }
}

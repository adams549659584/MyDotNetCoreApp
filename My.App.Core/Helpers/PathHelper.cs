using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace My.App.Core
{
    public class PathHelper
    {
        /// <summary>
        /// 返回指定虚拟路径相对应的物理文件夹路径。
        /// </summary>
        /// <param name="path">要为其获取物理路径的虚拟路径</param>
        /// <returns>与 path 相对应的物理文件夹路径。</returns>
        public static string MapPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("path IsNullOrWhiteSpace");
            }
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        /// <summary>
        /// 返回指定虚拟路径相对应的物理文件路径
        /// </summary>
        /// <param name="path">要为其获取物理路径的虚拟路径</param>
        /// <param name="fileName">要为其获取物理路径的文件名</param>
        /// <returns></returns>
        public static string MapFile(string path, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("fileName IsNullOrWhiteSpace");
            }
            var fullPath = MapPath(path);
            return Path.Combine(fullPath, fileName);
        }
    }
}

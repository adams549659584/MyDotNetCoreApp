using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace My.App.Core
{
    public class LogHelper
    {
        private static object logLock = new object();
        private static string LogPath
        {
            get
            {
                string path = string.Empty;
                //if (HttpContext.Current != null)
                //{
                //    path = Path.Combine(HttpContext.Current.Request.MapPath("~/"), "Logs");
                //}
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        //获取当前请求的浏览器信息
        private static string ClientBrowsers
        {
            get
            {
                //string browsers = string.Empty;
                //if (HttpContext.Current == null)
                //    return browsers;
                //HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
                //string aa = bc.Browser.ToString();
                //string bb = bc.Version.ToString();
                //browsers = aa + bb;
                //return browsers;
                return "";
            }
        }

        //获取客户端IP地址   
        private static string ClientIP
        {
            get
            {
                //string result = String.Empty;
                //if (HttpContext.Current == null)
                //    return result;
                //HttpRequest Request = HttpContext.Current.Request;
                string _userIP = "";

                //if (string.IsNullOrWhiteSpace(_userIP))
                //    _userIP = Request.ServerVariables["X-Forwarded-For"];

                //if (string.IsNullOrWhiteSpace(_userIP))
                //    _userIP = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                //if (string.IsNullOrWhiteSpace(_userIP))
                //    _userIP = Request.ServerVariables["REMOTE_ADDR"];

                //if (!string.IsNullOrWhiteSpace(_userIP))
                //{
                //    var ips = _userIP.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //    if (ips.Length > 0)
                //        _userIP = ips[0];
                //}
                //_userIP = _userIP.Trim().Replace(" ", string.Empty).Replace("　", string.Empty);
                return _userIP;
            }
        }

        private static void TaskLog(string log, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            string fileName = string.Format("{0}.log", DateTime.Now.ToString("yyyyMMddHH"));
            TaskLog(log, fileName, path, method, line);
        }

        private static void TaskLog(string log, string fileName, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            var sbLog = new StringBuilder();
            sbLog.AppendFormat("{0}:\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
            //if (HttpContext.Current != null)
            //{
            //    var request = HttpContext.Current.Request;
            //    sbLog.AppendFormat("请求路径:{0}\r\n", request.Url.AbsolutePath);
            //    sbLog.AppendFormat("请求类型：{0}\r\n", request.RequestType);
            //    sbLog.AppendFormat("客户端信息：{0}\r\n", ClientBrowsers);
            //    sbLog.AppendFormat("客户端IP：{0}\r\n", ClientIP);
            //}
            sbLog.AppendFormat("日志源路径:{0}\r\n", path);
            sbLog.AppendFormat("日志源函数:{0}\r\n", method);
            sbLog.AppendFormat("日志源行数:{0}\r\n", line);
            sbLog.AppendFormat("日志源信息:{0}\r\n\r\n", log);

            var logStr = sbLog.ToString();
            WriteLog(logStr, fileName);
#if (DEBUG)
            Console.WriteLine(logStr);
#else
            NotifyHelper.Weixin("MyDotNetCoreApp系统日志通知", logStr.Replace("\r\n", "     \r\n"));
#endif
        }

        private static void TaskLog(Exception ex, string fileName, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            var sbLog = new StringBuilder();
            sbLog.AppendFormat("\r\n请求访问时发生［{0}］异常，异常相关信息如下：", ex.GetType());
            sbLog.AppendFormat("\r\n异常描述：{0}", ex.Message);
            sbLog.AppendFormat("\r\n异 常 源：{0}", ex.Source);
            sbLog.AppendFormat("\r\n堆栈跟踪：\r\n{0}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                sbLog.AppendFormat("\r\n内含异常：{0}", ex.InnerException.GetType());
                sbLog.AppendFormat("\r\n异常描述：{0}", ex.InnerException.Message);
                sbLog.AppendFormat("\r\n异 常 源：{0}", ex.InnerException.Source);
                sbLog.AppendFormat("\r\n堆栈跟踪：\r\n{0}", ex.InnerException.StackTrace);
            }
            else
            {
                sbLog.Append("\r\n内含异常：无");
            }
            TaskLog(sbLog.ToString(), fileName, path, method, line);
        }

        private static void TaskLog(Exception ex, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            string fileName = string.Format("{0}.log", DateTime.Now.ToString("yyyyMMddHH"));
            TaskLog(ex, fileName, path, method, line);
        }

        public static void Log(string log, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            Task.Factory.StartNew(() =>
            {
                TaskLog(log, path, method, line);
            });
        }

        public static void Log2File(string log, string fileName, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            Task.Factory.StartNew(() =>
            {
                TaskLog(log, fileName, path, method, line);
            });
        }

        public static void Log2File(Exception ex, string fileName, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {

            Task.Factory.StartNew(() =>
            {
                TaskLog(ex, fileName, path, method, line);
            });
        }

        public static void Log(Exception ex, [CallerFilePath]string path = null, [CallerMemberName]string method = null, [CallerLineNumber]int line = 0)
        {
            Task.Factory.StartNew(() =>
            {
                TaskLog(ex, path, method, line);
            });
        }

        /// <summary>
        /// 纯log,不添加时间等
        /// </summary>
        /// <param name="log"></param>
        /// <param name="fileName"></param>
        public static void PureLog(string log, string fileName)
        {
            Task.Factory.StartNew(() =>
            {
                WriteLog(log, fileName);
            });
        }

        public static void WriteLog(string log, string fileName)
        {
            string fileFullName = Path.Combine(LogPath, fileName);
            lock (logLock)
            {
                string directoryName = System.IO.Path.GetDirectoryName(fileFullName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (StreamWriter sw = new StreamWriter(fileFullName, true, Encoding.UTF8))
                {
                    sw.Write(log);
                }
            }
        }
    }
}

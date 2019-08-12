using System;
using System.Collections.Generic;
using System.Text;

namespace My.App.Job
{
    public class ProxyConfigEnt
    {
        public string Code { get; set; }

        public string Desc { get; set; }

        /// <summary>
        /// {0} page
        /// </summary>
        public string FormatUrl { get; set; }

        /// <summary>
        /// 最大抓取页数
        /// </summary>
        public int MaxPage { get; set; }

        public string RowXPath { get; set; }

        /// <summary>
        /// 基于RowXpath
        /// </summary>
        public string IpXPath { get; set; }

        /// <summary>
        /// 基于RowXpath
        /// </summary>
        public string PortXPath { get; set; }

        public string LocationXPath { get; set; }

        /// <summary>
        /// 串联ip和端口
        /// </summary>
        public bool IsUnionIpAndPort { get; set; }

        public string NextPageXPath { get; set; }
    }
}

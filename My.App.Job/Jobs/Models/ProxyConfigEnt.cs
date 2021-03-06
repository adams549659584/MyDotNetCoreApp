﻿using System;
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

        /// <summary>
        /// 判断抓取失败的关键词,出现此关键词的重新抓取
        /// </summary>
        public string FailedKeyWords { get; set; }

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

        /// <summary>
        /// 当前a链接
        /// </summary>
        public string CurrentHref { get; set; }
        /// <summary>
        /// 链接以href为准
        /// </summary>
        public bool IsPageWithHref { get; set; }
    }
}

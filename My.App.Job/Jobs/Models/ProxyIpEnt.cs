using System;
using My.App.Core;

namespace My.App.Job
{
    public class ProxyIpEnt: IMongoEnt
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 是否支持https
        /// </summary>
        public bool IsSupportHttps { get; set; }

        /// <summary>
        /// 匿名程度
        /// </summary>
        public string Anonymity { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后校验时间
        /// </summary>
        public DateTime LastValidTime { get; set; }

        /// <summary>
        /// Http访问速度/毫秒
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Https访问速度/毫秒
        /// </summary>
        public double HttpsSpeed { get; set; }    

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 来源页编码
        /// </summary>
        /// <value></value>
        public string RefererSource { get; set; }
    }
}
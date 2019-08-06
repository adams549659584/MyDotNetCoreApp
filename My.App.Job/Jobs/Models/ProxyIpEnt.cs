using System;

namespace My.App.Job
{
    public class ProxyIpEnt
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }

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
        /// 类型 0-HTTP 1-HTTPS
        /// </summary>
        public int ProxyType { get; set; }

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
        /// 访问速度/毫秒
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
using System;
using System.Linq.Expressions;

namespace My.App.Core
{
    public class MongoFindOptions<T>
    {
        /// <summary>
        /// Gets or sets how many documents to return.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets how many documents to skip before returning the rest.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// 排序条件
        /// </summary>
        public Expression<Func<T, object>> SortConditions { get; set; }

        /// <summary>
        /// 是否倒序
        /// </summary>
        public bool IsDescending { get; set; }
    }
}
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace My.App.Core
{
    public class MongoDBServiceBase
    {
        protected IMongoDatabase MongoDB { get; }

        public MongoDBServiceBase(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            if (client == null)
            {
                throw new Exception($"mongo连接失败，连接字符串:{connectionString}");
            }
            MongoDB = client.GetDatabase(dbName);
        }

        #region SELECT
        /// <summary>
        /// 根据查询条件，获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<T> GetList<T>(Expression<Func<T, bool>> conditions = null)
        {
            var collection = MongoDB.GetCollection<T>(typeof(T).Name);
            if (conditions != null)
            {
                return collection.Find(conditions).ToList();
            }
            return collection.Find(_ => true).ToList();
        }
        #endregion

        #region INSERT/// <summary>
        /// 插入多条数据，数据用list表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<T> InsertMany<T>(List<T> list)
        {
            var collection = MongoDB.GetCollection<T>(typeof(T).Name);
            collection.InsertMany(list);
            return list;
        }
        #endregion
    }
}

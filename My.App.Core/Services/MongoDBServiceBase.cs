using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace My.App.Core
{
    public class MongoDBServiceBase
    {
        private static string DefaultConnectionString { 
            get
            {
                return $"mongodb://mongodb:mongo.123456.db@192.168.124.10:27017";
            } 
        }
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

        public MongoDBServiceBase(string dbName):this(DefaultConnectionString, dbName)
        {
        }

        #region SELECT
        /// <summary>
        /// 根据查询条件，获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<T>> GetList<T>(Expression<Func<T, bool>> conditions = null, MongoFindOptions<T> mongoOptions = null)
        {
            var collection = MongoDB.GetCollection<T>(typeof(T).Name);
            if (conditions == null)
            {
                conditions = _ => true;
            }
            FindOptions<T, T> options = null;
            if (mongoOptions != null)
            {
                options = new FindOptions<T, T>();
                options.Limit = mongoOptions.Limit;
                options.Skip = mongoOptions.Skip;
                if (mongoOptions.SortConditions !=null)
                {
                    if (mongoOptions.IsDescending)
                    {
                       options.Sort = new SortDefinitionBuilder<T>().Descending(mongoOptions.SortConditions); 
                    }
                    else
                    {
                        options.Sort = new SortDefinitionBuilder<T>().Ascending(mongoOptions.SortConditions);
                    }
                }
            }
            var result = await collection.FindAsync(conditions, options);
            return result.ToList();
        }
        #endregion

        #region INSERT/// <summary>
        /// 插入多条数据，数据用list表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<List<T>> Insert<T>(List<T> list)
        {
            var collection = MongoDB.GetCollection<T>(typeof(T).Name);
            await collection.InsertManyAsync(list);
            return list;
        }

        /// <summary>
        /// 插入单条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public async Task<T> Insert<T>(T ent)
        {
            var collection = MongoDB.GetCollection<T>(typeof(T).Name);
            await collection.InsertOneAsync(ent);
            return ent;
        }

        public async Task<T> Replace<T>(T ent) where T: IMongoEnt
        {
            var collection = MongoDB.GetCollection<T>(typeof(T).Name);
            var builder = Builders<T>.Filter;
            var filter = builder.Eq(x => x.Id, ent.Id);
            await collection.ReplaceOneAsync(filter, ent);
            return ent;
        }

        #endregion
    }
}

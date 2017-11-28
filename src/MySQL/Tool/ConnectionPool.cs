using System.Collections.Generic;
using System.Collections.Concurrent;

using Amib.Threading;

namespace MySQL.Tool
{
    public class ConnectionPool
    {
        private static ConcurrentDictionary<string, Queue> ConnPool;
        public static SmartThreadPool SmartPool { get; set; }        

        static ConnectionPool()
        {
            ConnPool = new ConcurrentDictionary<string, Queue>();
            SmartPool = new SmartThreadPool();
        }

        public static Connection Get(ExecuteConfigParams param, bool init)
        {
            Connection conn;
            var key = Connection.GetConnectionString(param);

            var items = ConnPool.GetOrAdd(key, new Queue(param));
            if (init == true)
                items.ReferenceCounter = items.ReferenceCounter + 1;
            if (param.PoolSize > items.PoolSize)
            {
                items.PoolSize = param.PoolSize;
            }
            if (items.TryDequeue(out conn))
            {
                return conn;
            }
            else
            {
                items.TryEnqueue();
                items.TryDequeue(out conn);
                return conn;
            }
        }

        public static void Put(KeyValuePair<string, Connection> item)
        {
            Queue queue;
            ConnPool.TryGetValue(item.Key, out queue);
            if (queue != null)
            {
                queue.TryEnqueue(item.Value);
            }
        }

        public static void Cleanup(ExecuteConfigParams param)
        {
            if (param == null)
                return;
            Queue items;
            var key = Connection.GetConnectionString(param);
            if (ConnPool.TryGetValue(key, out items))
            {
                items.ReferenceCounter = items.ReferenceCounter - 1;
            }
        }        
    }
}

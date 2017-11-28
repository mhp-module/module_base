using System.Threading;
using System.Collections.Concurrent;

namespace MySQL.Tool
{
    public class Queue
    {
        private ConcurrentQueue<Connection> queue;
        private ExecuteConfigParams param;

        public int poolSize;
        public int PoolSize
        {
            get { return this.poolSize; }
            set { Interlocked.Exchange(ref this.poolSize, value); }
        }

        public int referenceCounter;
        public int ReferenceCounter
        {
            get { return this.referenceCounter; }
            set { Interlocked.Exchange(ref this.referenceCounter, value); }
        }
        
        private int count = 0;
        public int Count
        {
            get { return this.count; }
            set { Interlocked.Exchange(ref this.count, value); }
        }
        public Queue()
        {
            this.queue = new ConcurrentQueue<Connection>();
        }

        public Queue(ExecuteConfigParams config)
        {
            this.param = config;
            this.queue = new ConcurrentQueue<Connection>();
            this.PoolSize = config.PoolSize;
            this.ReferenceCounter = 1;
        }

        public bool TryEnqueue(Connection conn)
        {
            this.queue.Enqueue(conn);
            return true;
        }

        public void TryEnqueue()
        {
            if (this.Count < this.PoolSize)
            {
                this.Count = this.Count + 1;
                var conn = new Connection(this.param);
                conn.InitConnection();
                TryEnqueue(conn);
            }
        }        

        public bool TryDequeue(out Connection conn)
        {
            this.queue.TryDequeue(out conn);
            if (conn == null)
                return false;
            return true;
        }
    }
}

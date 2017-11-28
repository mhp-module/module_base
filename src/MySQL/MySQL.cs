using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using fastJSON;

using IProc;
using IVal;
using MySQL.Tool;

namespace MySQL
{

    [ProcAttr(IsSingleInstance = false, Category = "Database")]
    public class MySQL : ProcModule, IDisposable
    {
        private ConnectionPool Pool { get; set; }
        private ExecuteConfigParams PreviousParameters { get; set; }

        public MySQL(ProcCtx procCtx) : base(procCtx) { }

        public override IValue ValidateInitialize(IValue param)
        {
            return new NullValue(null);
        }

        public override IValue ValidateExecute(IValue param)
        {
            return ExecuteConfigTool.CreateConfig(ExecuteConfigTool.ParseConfig(param));
        }

        public override void Initialize(IValue param)
        {
        }

        public override void Execute(IValue param)
        {       
            ConnectionPool.SmartPool.QueueWorkItem(() => { this.ThreadPoolHandler(param); });
        }

        public void ThreadPoolHandler(IValue param)
        {
            var config = ExecuteConfigTool.ParseConfig(param);
            bool init = false;
            if (!config.Equals(PreviousParameters))
            {
                init = true;
                PreviousParameters = config;
            }

            try
            {
                var Connection = ConnectionPool.Get(config, init);
                if (Connection != null)
                {
                    Connection.ExecuteCommand(config.Query, (IEnumerable<object> results) =>
                    {
                        ConnectionPool.Put(new KeyValuePair<string, Connection>(Connection.ConnectionString, Connection));
                        SendResults(results);
                    });
                }
                else
                {
                    var results = new Dictionary<string, object>() { { "status", "error" }, { "message", "Unable to process the query within allotted time frame." } };
                    this.ExecuteNextHandler(new DictionaryValue(new Dictionary<string, object>() { { "metric", results } }, null));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MySQL] exception detected : {ex.Message}");
            }
        }

        public void SendResults(IEnumerable<object> results)
        {
            var jsonResult = SqlToJson(results);
            if (jsonResult != null)
                this.ExecuteNextHandler(new DictionaryValue(new Dictionary<string, object>() { { "metric", jsonResult } }, null));
        }

        private object SqlToJson(object queryRes)
        {
            return JSON.Parse(JsonConvert.SerializeObject(queryRes));
        }

        public void Dispose()
        {
            ConnectionPool.Cleanup(PreviousParameters);
        }
    }
}


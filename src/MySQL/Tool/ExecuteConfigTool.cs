using System;
using System.Collections.Generic;

using IVal;

namespace MySQL.Tool
{
    public class ExecuteConfigParams
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Timeout { get; set; }
        public int PoolSize { get; set; }
        public string Query { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ExecuteConfigParams))
                return false;

            var right = (ExecuteConfigParams)obj;
            return right.Server == this.Server &&
                right.Port == this.Port &&
                right.Database == this.Database &&
                right.UserName == this.UserName &&
                right.Password == this.Password &&
                right.Timeout == this.Timeout &&
                right.PoolSize == this.PoolSize &&
                right.Query == this.Query;
        }

        public override int GetHashCode()
        {
            return this.Server.GetHashCode() ^ this.Port.GetHashCode() ^ 
                this.Database.GetHashCode() ^ this.UserName.GetHashCode() ^
                this.Password.GetHashCode() ^ this.Timeout.GetHashCode() ^
                this.PoolSize.GetHashCode() ^ this.Query.GetHashCode();
        }
    }

    class ExecuteConfigTool
    {
        public static ExecuteConfigParams CreateDefaultConfig()
        {
            return new ExecuteConfigParams()
            {
                Server = null,
                Port = 3306,
                Database = null,
                UserName = null,
                Password = null,
                Timeout = 100,
                PoolSize = 5,
                Query = null,
            };
        }
        
        public static IValue CreateConfig(ExecuteConfigParams config)
        {
            var retVal = new Dictionary<string, object>();
            var retAttr = new Dictionary<string, object>();

            retVal["Server"] = config.Server;
            if (config.Server == null || string.IsNullOrEmpty(config.Server))
                retAttr["Server"] = ValueFactory.CreateErrorAttribute("Server cannot be null or empty.");
            else
                retAttr["Server"] = ValueFactory.CreateNormalAttribute("success");

            retVal["Port"] = config.Port;
            if (config.Port > 0 && config.Port <= ushort.MaxValue)
                retAttr["Port"] = ValueFactory.CreateNormalAttribute("success");            
            else
                retAttr["Port"] = ValueFactory.CreateErrorAttribute("Port number should have a valid value.");

            retVal["Database"] = config.Database;
            if (config.Database == null || string.IsNullOrEmpty(config.Database))
                retAttr["Database"] = ValueFactory.CreateErrorAttribute("Database cannot be null or empty.");
            else
                retAttr["Database"] = ValueFactory.CreateNormalAttribute("success");

            retVal["UserName"] = config.UserName;
            if (config.UserName == null || string.IsNullOrEmpty(config.UserName))
                retAttr["UserName"] = ValueFactory.CreateWarningAttribute("Database server may require authentication.");
            else
                retAttr["UserName"] = ValueFactory.CreateNormalAttribute("success");

            retVal["Password"] = config.Password;
            if (config.Password == null || string.IsNullOrEmpty(config.Password))
                retAttr["Password"] = ValueFactory.CreateWarningAttribute("Database server may require authentication.");
            else
                retAttr["Password"] = ValueFactory.CreateNormalAttribute("success");

            retVal["Timeout"] = config.Timeout;
            if (config.Timeout <= 0)
                retAttr["Timeout"] = ValueFactory.CreateWarningAttribute("Query timeout should have a valid value.");            
            else if (config.Timeout == 100)
                retAttr["Timeout"] = ValueFactory.CreateWarningAttribute("Query may time out with the current settings.");
            else
                retAttr["Timeout"] = ValueFactory.CreateNormalAttribute("success");

            retVal["PoolSize"] = config.PoolSize;
            if (config.PoolSize <= 0)
                retAttr["PoolSize"] = ValueFactory.CreateWarningAttribute("Connection pool size should have a valid value.");
            else if (config.PoolSize == 5)
                retAttr["PoolSize"] = ValueFactory.CreateWarningAttribute("Query may not execute within the time frame if the pool size has a small value.");
            else
                retAttr["PoolSize"] = ValueFactory.CreateNormalAttribute("success");

            retVal["Query"] = config.Query;
            if (config.Query == null || string.IsNullOrEmpty(config.Query))
                retAttr["Query"] = ValueFactory.CreateErrorAttribute("Query cannot be null or empty.");
            else
                retAttr["Query"] = ValueFactory.CreateNormalAttribute("success");

            return new DictionaryValue(retVal, retAttr);
        }

        public static ExecuteConfigParams ParseConfig(IValue parameter)
        {
            var param = parameter.GetVal() as Dictionary<string, object>;
            if (param == null || param.Count <= 0) return ExecuteConfigTool.CreateDefaultConfig();

            return new ExecuteConfigParams()
            {
                Server = ExecuteConfigTool.ParseStringConfig(param, "Server"),
                Port = ExecuteConfigTool.ParseIntConfig(param, "Port"),
                Database = ExecuteConfigTool.ParseStringConfig(param, "Database"),
                UserName = ExecuteConfigTool.ParseStringConfig(param, "UserName"),
                Password = ExecuteConfigTool.ParseStringConfig(param, "Password"),
                Timeout = ExecuteConfigTool.ParseIntConfig(param, "Timeout"),
                PoolSize = ExecuteConfigTool.ParseIntConfig(param, "PoolSize"),
                Query = ExecuteConfigTool.ParseStringConfig(param, "Query")
            };
        }

        private static int ParseIntConfig(Dictionary<string, object> config, string key)
        {
            var retValue = -1;

            if (config.ContainsKey(key))
            {
                var value = config[key];

                if (value is Int64)
                    retValue = Convert.ToInt32((Int64)value);
                else int.TryParse(value.ToString(), out retValue);
            }

            return retValue;
        }

        private static string ParseStringConfig(Dictionary<string, object> config, string key)
        {
            if (config.ContainsKey(key))
                return config[key].ToString();
            return string.Empty;
        }
    }
}

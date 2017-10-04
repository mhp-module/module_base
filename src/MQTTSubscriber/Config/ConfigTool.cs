using IVal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTSubscriber.Config
{
    class InitConfig
    {
        public string Host { get; set; }
        public string Topic { get; set; }
    }

    class ConfigTool
    {
        public static InitConfig CreateDefaultConfig()
        {
            return new InitConfig()
            {
                Host = string.Empty,
                Topic = string.Empty
            };
        }

        public static IValue CreateConfig(InitConfig config)
        {
            var retVal = new Dictionary<string, object>();
            var retAttr = new Dictionary<string, object>();

            retVal["Host"] = config.Host;
            if (string.IsNullOrEmpty(config.Host))
                retAttr["Host"] = ValueFactory.CreateErrorAttribute("Host field cannot be null or empty");
            else
                retAttr["Host"] = ValueFactory.CreateNormalAttribute("success");

            retVal["Topic"] = config.Topic;
            if (string.IsNullOrEmpty(config.Topic))
                retAttr["Topic"] = ValueFactory.CreateErrorAttribute("Topic field cannot be null or empty");
            else
                retAttr["Topic"] = ValueFactory.CreateNormalAttribute("success");
            
            return new DictionaryValue(retVal, retAttr);
        }

        public static InitConfig ParseConfig(IValue param)
        {
            var paramVal = param.GetVal() as Dictionary<string, object>;
            if (paramVal == null || paramVal.Count <= 0) return ConfigTool.CreateDefaultConfig();

            return new InitConfig()
            {
                Host = ConfigTool.ParseStringConfig(paramVal, "Host"),
                Topic = ConfigTool.ParseStringConfig(paramVal, "Topic")
            };
        }
        
        private static string ParseStringConfig(Dictionary<string, object> config, string key)
        {
            if (config.ContainsKey(key))
            {
                var targetConfig = config[key] as string;
                if (!string.IsNullOrEmpty(targetConfig)) return targetConfig;
            }
            return string.Empty;
        }
    }
}

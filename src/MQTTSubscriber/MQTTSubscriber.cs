using fastJSON;
using IProc;
using IVal;
using MQTTSubscriber.Config;
using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTTSubscriber
{
    [ProcAttr(IsSingleInstance = false, Category = "MQTT")]
    public class MQTTSubscriber : ProcModule, IDisposable
    {
        private InitConfig config = null;
        private MqttClient mqttClient = null;

        public MQTTSubscriber(ProcCtx procCtx) : base(procCtx)
        {
        }

        public override IValue ValidateInitialize(IValue param)
        {
            return ConfigTool.CreateConfig(ConfigTool.ParseConfig(param));
        }

        public override IValue ValidateExecute(IValue param)
        {
            return new NullValue(null);
        }

        public override void Initialize(IValue param)
        {
            this.config = ConfigTool.ParseConfig(param);
            this.mqttClient_ConnectionClosed(null, null);
        }
        
        public override void Execute(IValue param)
        {
        }

        public void Dispose()
        {
            if (mqttClient != null)
            {
                mqttClient.MqttMsgPublishReceived -= this.mqttClient_MqttMsgPublishReceived;
                mqttClient.ConnectionClosed -= this.mqttClient_ConnectionClosed;
                mqttClient.Disconnect();
            }
        }
        
        void mqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                var contentString = Encoding.UTF8.GetString(e.Message);
                object parsedJson = null;

                try { parsedJson = JSON.Parse(contentString); }
                catch { Console.WriteLine("[MqttSubscriber] only json format is supported. content : " + contentString); }

                if(parsedJson != null && (parsedJson is List<object> || parsedJson is Dictionary<string, object>))
                    this.ExecuteNextHandler(new DictionaryValue(new Dictionary<string, object>() { { "metric", parsedJson } }, null));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MqttSubscriber][MsgReceived] exception occurred : " + ex.Message);
            }
        }

        private void mqttClient_ConnectionClosed(object sender, EventArgs e)
        {
            try
            {
                this.Dispose();

                var client = new MqttClient(this.config.Host);
                client.MqttMsgPublishReceived += this.mqttClient_MqttMsgPublishReceived;
                client.ConnectionClosed += this.mqttClient_ConnectionClosed;
                client.Connect(Guid.NewGuid().ToString());
                client.Subscribe(new string[] { this.config.Topic }, new byte[] { 2 });
                
                this.mqttClient = client;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MqttSubscriber][OnConnect] exception occurred : " + ex.Message);
            }
        }
    }
}

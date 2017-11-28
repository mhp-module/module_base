using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

using MySql.Data.MySqlClient;

namespace MySQL.Tool
{
    public class Connection
    {
        private MySqlConnection DbConnection { get; set; }
        private ExecuteConfigParams Config { get; set; }

        public Connection()
        {
        }
        
        public Connection(ExecuteConfigParams param)
        {
            this.Config = param;
        }

        public string ConnectionString
        {
            get
            {
                return $@"Server={this.Config.Server};Database={this.Config.Database};Port={this.Config.Port};uid={this.Config.UserName};pwd={this.Config.Password};Default Command Timeout={this.Config.Timeout}";
            }
        }

        public static string GetConnectionString(ExecuteConfigParams config)
        {
            return $@"Server={config.Server};Database={config.Database};Port={config.Port};uid={config.UserName};pwd={config.Password};Default Command Timeout={config.Timeout}";
        }

        public Connection InitConnection()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.ConnectionString))
                {
                    this.DbConnection = new MySqlConnection(this.ConnectionString);
                    this.DbConnection.Open();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MySQL] exception detected : {ex.Message}");
                return null;
            }
            return this;
        }

        public void ExecuteCommand(string query, Action<IEnumerable<object>> sendResults)
        {
            var results = new List<object>();
            try
            {
                if (this.DbConnection == null || this.DbConnection.State != ConnectionState.Open)
                {
                    Console.WriteLine($"[MySQL] exception detected : Connection is closed.");
                    return;
                }
                using (var command = new MySqlCommand(query, this.DbConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                        foreach (IDataRecord record in reader as IEnumerable)
                        {
                            var expando = new ExpandoObject() as IDictionary<string, object>;
                            foreach (var name in names)
                                expando[name] = record[name];                                

                            results.Add((object)expando.Values);
                        }
                    }
                }
                sendResults(results);                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MySQL] exception detected : {ex.Message}");
            }
        }
    }
}

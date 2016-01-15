using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SqlEtl.Entities;

namespace SqlEtl.Implementation
{
    internal abstract class BulkCopyBaseHelper
    {
        protected Dictionary<string, string> Logic;
        protected ScopeTableCollection ScopeDescription;
        protected string[] ScopeTables;
        protected BulkCopySession SessionData;

        protected ScopeTable GetTable(string tableName)
        {
            return ScopeDescription.FirstOrDefault(p => string.Equals(p.Name, tableName, StringComparison.CurrentCultureIgnoreCase));
        }

        protected string GetLogic(string tableName)
        {
            string v;
            if (Logic.TryGetValue(tableName, out v))
            {
                return v;
            }
            throw new Exception("Script does not exists.");
        }

        protected SqlCommand SqlTextCommand(string tableName)
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandTimeout = SessionData.ConnectionTimeout,
                CommandText = GetLogic(tableName)
            };
            return cmd;
        }
    }
}
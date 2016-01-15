using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SqlEtl.Entities;

namespace SqlEtl.Implementation
{
    internal class BulkCopyBaseProvider : BulkCopyBaseHelper
    {
        internal BulkCopyBaseProvider(BulkCopySession bcSessionData, Dictionary<string, string> logic,
            ScopeTableCollection scopeDescription)
        {
            SessionData = bcSessionData;
            ScopeDescription = scopeDescription;
            Logic = logic;
        }

        internal BulkInsertProvider GetProvider()
        {
            var provider = new BulkInsertProvider();
            var connection = new SqlConnection(SessionData.ConnectionString);
            SessionData.ConnectionTimeout = connection.ConnectionTimeout > 36000 ? connection.ConnectionTimeout : 36000;
            provider.Connection = connection;
            provider.BatchSize = SessionData.BatchSize;
            var tableInfo = GetTable(SessionData.TableName);

            if (tableInfo == null) return provider;
            provider.TableName = tableInfo.Name;
            var cmd = SqlTextCommand(tableInfo.Name);
            cmd.Parameters.Add("@batchSize", SqlDbType.Int).Value = SessionData.BatchSize;
            cmd.Parameters.Add("@batchIndex", SqlDbType.Int).Value = SessionData.BatchIndex;
            provider.SelectBulkRows = cmd;

            return provider;
        }
    }
}
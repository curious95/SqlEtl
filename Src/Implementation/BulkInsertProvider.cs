using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using SqlEtl.Fault;
using SqlEtl.Interfaces;

namespace SqlEtl.Implementation
{
    public class BulkInsertProvider : IBulkInsertProvider
    {
        #region Events

        private void bc_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Public Members

        public DataSet GetBulkRows()
        {
            var ds = new DataSet(TableName);
            using (var con = new SqlConnection(Connection.ConnectionString))
            {
                using (var cmd = SelectBulkRows)
                {
                    var policy = new RetryPolicy<SqlTransientErrorDetectionStrategy>(2, TimeSpan.FromSeconds(5));
                    cmd.Connection = con;
                    con.Open();
                    var adapter = new SqlDataAdapter(cmd);
                    policy.ExecuteAction(() => adapter.Fill(ds, TableName));
                }
            }
            return ds;
        }

        public int InsertBulkRows(DataSet data)
        {
            SourceData = data;
            var val = 0;
            if (SourceData == null) return val;
            val = SourceData.Tables[0].Rows.Count;
            using (var con = new SqlConnection(Connection.ConnectionString))
            {
                using (var bc = new SqlBulkCopy(con))
                {
                    bc.BatchSize = BatchSize;
                    bc.NotifyAfter = BatchSize;
                    bc.BulkCopyTimeout = (con.ConnectionTimeout > 3600) ? con.ConnectionTimeout : 3600;
                    foreach (DataColumn c in SourceData.Tables[0].Columns)
                    {
                        bc.ColumnMappings.Add(c.ColumnName, c.ColumnName);
                    }
                    bc.SqlRowsCopied += bc_SqlRowsCopied;
                    bc.DestinationTableName = TableName;
                    con.Open();
                    bc.WriteToServer(SourceData.Tables[0]);
                }
            }
            return val;
        }

        public IDbConnection Connection { get; set; }
        public SqlCommand SelectBulkRows { get; set; }
        public DataSet SourceData { get; set; }
        public string TableName { get; set; }
        public int BatchSize { get; set; }

        #endregion
    }
}
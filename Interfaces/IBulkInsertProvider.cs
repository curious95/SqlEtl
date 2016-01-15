using System.Data;
using System.Data.SqlClient;

namespace SqlEtl.Interfaces
{
    public interface IBulkInsertProvider
    {
        SqlCommand SelectBulkRows { get; set; }
        IDbConnection Connection { get; set; }
        int BatchSize { get; set; }
        DataSet SourceData { get; set; }
        string TableName { get; set; }
        DataSet GetBulkRows();
        int InsertBulkRows(DataSet data);
    }
}
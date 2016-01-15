using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace SqlEtl.Fault
{
    public class SqlTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            var sqlException = ex as SqlException;
            return sqlException != null && sqlException.Errors.Cast<SqlError>().Any(error => error.Number == 50000);
        }
    }
}
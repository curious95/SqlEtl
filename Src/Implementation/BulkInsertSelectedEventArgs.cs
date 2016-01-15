using System;
using System.Data;

namespace SqlEtl.Implementation
{
    public class BulkInsertSelectedEventArgs : EventArgs
    {
        public BulkInsertSelectedEventArgs(DataSet d)
        {
            ProgressTable = d;
            Inserts = d.Tables[0].Rows.Count;
        }

        public DataSet ProgressTable { get; protected set; }
        public int Inserts { get; protected set; }
    }
}

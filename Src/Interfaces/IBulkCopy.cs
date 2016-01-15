using System.Collections.Generic;
using System.Data;
using SqlEtl.Implementation;

namespace SqlEtl.Interfaces
{
    internal interface IBulkCopy
    {
        void Initialize();
        BulkInsertProvider GetProvider(string bcTable, int index);
        DataSet GetRowCount();
        Dictionary<string, string> GetSkipList();
        void Finalize(Dictionary<string, object> param);
        void BuildObjects();
        void ExecScript(string fileName);
        Dictionary<string, object> GetSourceScript();
        void SetSourceScript(Dictionary<string, object> sourceScript);
    }
}
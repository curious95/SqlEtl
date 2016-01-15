using System.Collections.Generic;
using System.Text;
using SqlEtl.Entities;
using SqlEtl.Script.Template;

namespace SqlEtl.Script
{
    internal class ScriptEngine
    {
        private BulkCopySession _request;
        private readonly ScopeTableCollection _scopeDescription;

        internal ScriptEngine(BulkCopySession request, ScopeTableCollection scopeDescription)
        {
            _request = request;
            _scopeDescription = scopeDescription;
        }

        internal Dictionary<string, object> GetScript()
        {
            var provisioningTSql = new Dictionary<string, object>();
            var logic = new Dictionary<string, string>();
            var removeExisting = new StringBuilder();
            removeExisting.Append(SqlTemplate.Get("DisableConstraintCheck"));
            removeExisting.Append(SqlTemplate.Get("DisableAllTrigger"));
            foreach (var ti in _scopeDescription)
            {
                var script = new ScriptLogic(ti);
                logic.Add(ti.Name, script.SelectChanges);
            }
            for (var i = _scopeDescription.Count - 1; i >= 0; i--)
                removeExisting.Append("delete from  " + _scopeDescription[i].Name + ";");
            removeExisting.Append(SqlTemplate.Get("EnableAllTrigger"));
            removeExisting.Append(SqlTemplate.Get("EnableConstraintCheck"));
            provisioningTSql.Add("Logic", logic);
            provisioningTSql.Add("Truncate", removeExisting.ToString());
            return provisioningTSql;
        }
    }
}
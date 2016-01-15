using System.Linq;
using SqlEtl.Entities;
using SqlEtl.Helpers;
using SqlEtl.Script.Template;

namespace SqlEtl.Script
{
    internal class ScriptLogic
    {
        private readonly string _allColumnsParam = string.Empty;
        private readonly string _allColumnsParamWithType = string.Empty;
        private readonly string _clusterKeys = string.Empty;
        private readonly string _selectFields = string.Empty;
        private readonly string _tableName;
        private readonly string _withEncryption = "";

        internal ScriptLogic(ScopeTable ti)
        {
            string type;
            string length;
            var primaryScopeColumn = from ScopeColumn ci in ti.Columns where ci.Primary select ci;
            var nonPrimaryScopeColumn = from ScopeColumn ci in ti.Columns where (ci.Primary == false) select ci;
            _tableName = ti.Name;
            foreach (var ci in primaryScopeColumn)
            {
                type = Runtime.GetSqlType(ci.Type);
                length = Runtime.GetLength(ci.Type.ToString(), ci.Length);

                if (string.IsNullOrEmpty(_clusterKeys))
                {
                    _clusterKeys = "[" + ci.Name + "]";
                }
                else
                {
                    _clusterKeys += ",[" + ci.Name + "]";
                }
                _selectFields += "t.[" + ci.Name + "],";
                if (string.IsNullOrEmpty(_allColumnsParamWithType))
                    _allColumnsParamWithType = "@" + ci.Name + " " + type + length;
                else
                    _allColumnsParamWithType += ",@" + ci.Name + " " + type + length;
                if (string.IsNullOrEmpty(_allColumnsParam))
                    _allColumnsParam = "@" + ci.Name;
                else
                    _allColumnsParam += ",@" + ci.Name;
            }

            foreach (var ci in nonPrimaryScopeColumn)
            {
                type = Runtime.GetSqlType(ci.Type);
                length = Runtime.GetLength(ci.Type.ToString(), ci.Length);
                _selectFields += "c.[" + ci.Name + "],";
                if (string.IsNullOrEmpty(_allColumnsParamWithType))
                    _allColumnsParamWithType = "@" + ci.Name + " " + type + length;
                else
                    _allColumnsParamWithType += ",@" + ci.Name + " " + type + length;
                if (string.IsNullOrEmpty(_allColumnsParam))
                    _allColumnsParam = "@" + ci.Name;
                else
                    _allColumnsParam += ",@" + ci.Name;
            }
            _selectFields = _selectFields.TrimEnd(char.Parse(","));
        }

        internal string SelectChanges => string.Format(SqlTemplate.Get("SelectChanges"), "dbo", "bc", _tableName, _withEncryption,
            _selectFields.Replace("t.", "c."), _clusterKeys);
    }
}
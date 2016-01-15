using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using SqlEtl.Entities;
using SqlEtl.Enums;
using SqlEtl.Helpers;
using SqlEtl.Script;
using SqlEtl.Script.Template;

namespace SqlEtl.Implementation
{
    public sealed class BulkCopyProvider
    {
        #region member variables

        private const string SysIndexSql = "SELECT LOWER(so.name) as TableName, ddps.row_count as [RowCount] FROM sys.objects so JOIN sys.indexes si ON si.OBJECT_ID = so.OBJECT_ID JOIN sys.dm_db_partition_stats AS ddps ON si.OBJECT_ID = ddps.OBJECT_ID  AND si.index_id = ddps.index_id WHERE so.type='U' and si.index_id < 2  AND so.is_ms_shipped = 0 {0} ORDER BY ddps.row_count DESC";

        private BulkCopySession _request;
        private Dictionary<string, object> _script = new Dictionary<string, object>();
        private Dictionary<string, string> _logic = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _skipList = new Dictionary<string, string>();
        private ScopeTableCollection _scopeDescription;
        private string[] _scopeTables;
        private DataSet _ds;
        private Dictionary<string, object> _initializedParam = new Dictionary<string, object>();
        private string[] _skipTables = { };

        #endregion

        #region ctor

        public BulkCopyProvider(BulkCopySession request)
        {
            _request = request;
        }

        #endregion

        #region Public Members

        public void BuildObjects()
        {
            var smo = new SqlSmo(_request.ConnectionString, Runtime.Path);
            smo.GenerateScript();
        }

        public void Initialize(Dictionary<string, object> param)
        {
            _initializedParam = param;
            ScriptType[] scopeScript = { ScriptType.None };
            object v;
            if (_initializedParam.TryGetValue("ScriptType", out v))
            {
                scopeScript = v as ScriptType[];
            }
            if (_initializedParam.TryGetValue("SkipTables", out v))
            {
                _skipTables = v as string[];
            }
            if (_ds == null)
            {
                _ds = GetRowCount();
                var query = _ds.Tables[0].AsEnumerable();
                _scopeTables = query.Select(dr => dr.Field<string>("tablename")).ToArray();
            }
            _scopeDescription = ExtractSchema(_scopeTables);
            SetupScript(scopeScript);
        }

        public Dictionary<string, object> GetSourceScript()
        {
            var result = new Dictionary<string, object>
            {
                {"ScopeDescription", _scopeDescription},
                {"ScriptLogic", _logic}
            };
            return result;
        }

        public void SetSourceScript(Dictionary<string, object> dict)
        {
            _scopeDescription = dict["ScopeDescription"] as ScopeTableCollection;
            _logic = dict["ScriptLogic"] as Dictionary<string, string>;
        }

        public BulkInsertProvider CreateProvider(BulkCopySession request)
        {
            _request = request;
            return new BulkCopyBaseProvider(request, _logic, _scopeDescription).GetProvider();
        }

        public DataSet GetRowCount()
        {
            if (_ds != null) return _ds;
            _ds = new DataSet();
            using (var con = new SqlConnection(_request.ConnectionString))
            {
                using (var cmd = new SqlCommand(BuildCriteria(), con))
                {
                    con.Open();
                    var adapter = new SqlDataAdapter { SelectCommand = cmd };
                    adapter.Fill(_ds);
                }
            }
            return _ds;
        }

        public Dictionary<string, string> GetSkipList()
        {
            return _skipList;
        }

        public void Finalize(Dictionary<string, object> param)
        {
            object v;
            var enableConstraint = false;
            if (param.TryGetValue("EnableConstraintCheck", out v))
            {
                enableConstraint = Convert.ToBoolean(v);
            }
            if (enableConstraint)
            {
                ExecuteTemplate("EnableConstraintCheck");
            }
        }

        #endregion

        #region Private Helper

        private void ExecuteTemplate(string template)
        {
            using (var con = new SqlConnection(_request.ConnectionString))
            {
                using (var cmd = new SqlCommand(SqlTemplate.Get(template), con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SetupScript(ScriptType[] scriptOption)
        {
            if (_script.Count == 0)
            {
                _script = (new ScriptEngine(_request, _scopeDescription)).GetScript();
            }
            object sql;
            if (_script.TryGetValue("Logic", out sql))
            {
                _logic = sql as Dictionary<string, string>;
            }
            foreach (var option in scriptOption)
            {
                switch (option)
                {
                    case ScriptType.Recovery:
                        UpdateRecoveryModel(RecoveryModel.BulkLogged);
                        break;
                    case ScriptType.Truncate:
                        if (_script.TryGetValue("Truncate", out sql))
                        {
                            ExecScript(sql as string, true);
                        }
                        break;
                    case ScriptType.DisableConstraint:
                        ExecuteTemplate("DisableConstraintCheck");
                        break;
                    case ScriptType.None:
                        break;
                    case ScriptType.Select:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void UpdateRecoveryModel(RecoveryModel recoveryModel)
        {
            try
            {
                using (var con = new SqlConnection(_request.ConnectionString))
                {
                    var sql = $"alter database [{con.Database}] set recovery {recoveryModel}";
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandTimeout = _request.ConnectionTimeout;
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.Text;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //Ok to skip
            }
        }

        public void ExecScript(string fileName)
        {
            ExecScript(File.ReadAllText(fileName), false);
        }

        private void ExecScript(string sqlscript, bool tranRequired)
        {
            SqlTransaction tran = null;
            var lines = Runtime.RegEx.Split(sqlscript);
            using (var con = new SqlConnection(_request.ConnectionString))
            {
                con.Open();
                if (tranRequired)
                    tran = con.BeginTransaction();
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandTimeout = _request.ConnectionTimeout;
                    if (tranRequired)
                        cmd.Transaction = tran;
                    foreach (var line in lines)
                    {
                        if (line.Length > 0)
                        {
                            cmd.CommandText = line;
                            cmd.CommandType = CommandType.Text;

                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException)
                            {
                                if (tranRequired)
                                    tran.Rollback();
                                if (tranRequired)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
                if (tranRequired)
                    tran.Commit();
            }
        }

        private ScopeTableCollection ExtractSchema(string[] schemaList)
        {
            var tablesInfo = new ScopeTableCollection();
            var extractSchema = SqlTemplate.Get("ExtractSchema");
            using (var con = new SqlConnection(_request.ConnectionString))
            {
                using (var cmd = new SqlCommand())
                {
                    con.Open();
                    foreach (var sequence in schemaList)
                    {
                        try
                        {
                            cmd.Parameters.Clear();
                            cmd.Connection = con;
                            cmd.CommandText = extractSchema;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@objname", SqlDbType.NVarChar).Value = sequence;
                            ScopeColumnCollection tableInfo;
                            using (var reader = cmd.ExecuteReader())
                            {
                                tableInfo = new ScopeColumnCollection();
                                if (!reader.HasRows)
                                    throw new Exception($"Table {sequence} does not exists.");
                                while (reader.Read())
                                {
                                    tableInfo.Add(new ScopeColumn
                                    {
                                        Name = reader.GetString(0),
                                        Type = Runtime.GetType(reader.GetString(1)),
                                        Length = reader.GetInt32(2),
                                        Primary = Runtime.CheckInternalKey(_request.CustomKeyDefinitions, sequence, reader.GetString(0), reader.GetBoolean(4))
                                    });
                                }
                            }
                            var primaryExists = (from ScopeColumn ci in tableInfo where ci.Primary select ci.Primary).FirstOrDefault();
                            if (primaryExists)
                                tablesInfo.Add(new ScopeTable { Name = sequence, Columns = tableInfo });
                            else
                            {
                                if (!_skipList.ContainsKey(sequence))
                                {
                                    _skipList.Add(sequence, "Table needs primary key or clustered index.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!_skipList.ContainsKey(sequence))
                            {
                                _skipList.Add(sequence, ex.ToString());
                            }
                        }
                    }
                }
                _scopeDescription = tablesInfo;
                return tablesInfo;
            }
        }

        private string BuildCriteria()
        {
            var q = "";
            if (_skipTables != null && _skipTables.Any())
            {
                q = String.Join(q, _skipTables.Where(p => p.Contains("%")).Select(p => " and so.name not like '" + p + "' "));
            }
            return String.Format(SysIndexSql, q);
        }

        #endregion
    }
}
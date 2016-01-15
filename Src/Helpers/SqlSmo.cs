using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace SqlEtl.Helpers
{
    public class SqlSmo
    {
        public SqlSmo(string connectionString, string scriptPath)
        {
            ConnString = connectionString;
            ScriptPath = scriptPath;
        }

        private string ConnString { get; }
        private string ScriptPath { get; }

        private void BuildScript(Scripter scripter, List<Urn> urns, string fileName)
        {
            var sc = scripter.Script(urns.ToArray());
            using (var writer = File.CreateText(ScriptPath + fileName))
            {
                foreach (var st in sc)
                {
                    writer.WriteLine(st.Trim('\r', '\n') + "\r\nGO\r\n");
                }
            }
        }

        public void GenerateScript()
        {
            var con = new ServerConnection {ConnectionString = ConnString};
            string databaseName;
            using (var csrt = new SqlConnection(ConnString))
            {
                databaseName = csrt.Database;
            }
            var srv = new Server(con);
            var db = srv.Databases[databaseName];
            try
            {
                var scripter = new Scripter(srv)
                {
                    Options =
                    {
                        AppendToFile = false,
                        ToFileOnly = false,
                        ScriptDrops = false,
                        Indexes = true,
                        DriAllConstraints = true,
                        Triggers = true,
                        FullTextIndexes = true,
                        NonClusteredIndexes = true,
                        NoCollation = true,
                        Bindings = true,
                        SchemaQualify = true,
                        IncludeDatabaseContext = false,
                        AnsiPadding = true,
                        FullTextStopLists = false,
                        IncludeIfNotExists = true,
                        ScriptBatchTerminator = true,
                        ExtendedProperties = true,
                        ClusteredIndexes = true,
                        FullTextCatalogs = true,
                        SchemaQualifyForeignKeysReferences = true,
                        XmlIndexes = true,
                        IncludeHeaders = false,
                        WithDependencies = true,
                        DriAll = true,
                        DriAllKeys = true
                    },
                    PrefetchObjects = true
                };
              
                scripter.Options.NoTablePartitioningSchemes = true;
                scripter.Options.ScriptSchema = true;
                scripter.Options.ScriptData = false;
                scripter.Options.NoCommandTerminator = false;
                scripter.Options.AllowSystemObjects = false;
                scripter.Options.Permissions = true;
                scripter.Options.SchemaQualify = true;
                scripter.Options.AnsiFile = true;
                scripter.Options.EnforceScriptingOptions = true;

                var urns = new List<Urn>();
                var schemaBuilder = new StringBuilder();
                foreach (Schema s in db.Schemas)
                {
                    if (s.IsSystemObject == false)
                    {
                        schemaBuilder.AppendLine(s.Script()[0]);
                        schemaBuilder.AppendLine("GO");
                    }
                }
                using (var writer = File.CreateText(ScriptPath + "schema.sql"))
                {
                    writer.WriteLine(schemaBuilder.ToString());
                }
                //Tables
                foreach (Schema s in db.Schemas)
                {
                    if (s.IsSystemObject == false)
                    {
                        //s.
                    }
                }
                BuildScript(scripter, urns, "tables.sql");
                //Tables
                foreach (Table tb in db.Tables)
                {
                    if (tb.IsSystemObject == false)
                    {
                        urns.Add(tb.Urn);
                    }
                }
                BuildScript(scripter, urns, "tables.sql");

                //Stored Procs
                urns.Clear();
                foreach (StoredProcedure sp in db.StoredProcedures)
                {
                    if (sp.IsSystemObject == false)
                    {
                        urns.Add(sp.Urn);
                    }
                }
                BuildScript(scripter, urns, "procs.sql");

                //Views
                urns.Clear();
                foreach (View view in db.Views)
                {
                    if (view.IsSystemObject == false)
                    {
                        // View is not a system object, so add it.
                        urns.Add(view.Urn);
                    }
                }
                BuildScript(scripter, urns, "views.sql");


                using (var writer = File.CreateText(ScriptPath + "fks.sql"))
                {
                    foreach (Table tab in db.Tables)
                    {
                        foreach (ForeignKey f in tab.ForeignKeys)
                        {
                            var fks = f.Script();
                            foreach (var st in fks)
                            {
                                writer.WriteLine(st.Trim('\r', '\n') + "\r\nGO\r\n");
                            }
                        }
                    }
                }
                using (var writer = File.CreateText(ScriptPath + "indexes.sql"))
                {
                    foreach (Table tab in db.Tables)
                    {
                        foreach (Index f in tab.Indexes)
                        {
                            var fks = f.Script();
                            foreach (var st in fks)
                            {
                                writer.WriteLine(st.Trim('\r', '\n') + "\r\nGO\r\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Couldn't generate script." + ex);
            }
        }
    }
}
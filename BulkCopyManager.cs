// <copyright file="BulkCopyManager.cs" company="">
// Copyright (c) 2016 All Right Reserved,
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Udaiappa Ramachandran</author>
// <email>udaiappa@gmail.com</email>
// <date>2016-01-15</date>
// <summary></summary>

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SqlEtl.Entities;
using SqlEtl.Enums;
using SqlEtl.Helpers;
using SqlEtl.Implementation;
using SqlEtl.Logging;

namespace SqlEtl
{
    public class BulkCopyManager : IDisposable
    {
        #region member variable

        private uint _changeApplied, _changeFailed, _changeTotal;
        private DataSource _sourceDb;
        private DataSource _destinationDb;
        private BulkCopyResponse _response;
        private BulkCopyProgressEventArgs _eventArgs = new BulkCopyProgressEventArgs();
        private readonly DateTime _startTime = DateTime.Now;

        #endregion

        #region Events

        public delegate void BulkCopyProgressEvent(object sender, BulkCopyProgressEventArgs e);

        public event BulkCopyProgressEvent BulkCopyProgress;

        #endregion

        #region public members

        public BulkCopyResponse BulkCopy(BulkCopyRequest request)
        {
            var sourceParam = new Dictionary<string, object>();
            var destParam = new Dictionary<string, object>();
            try
            {
                if (request.BatchSize <= 0)
                {
                    request.BatchSize = 100;
                }
                _eventArgs = new BulkCopyProgressEventArgs {TotalWork = 0, CompletedWork = 0, Result = string.Empty};

                sourceParam.Add("ScriptType", new[] {ScriptType.Select});
                sourceParam.Add("SkipTables", request.Skip);
                _sourceDb = new DataSource(request, Position.Source, sourceParam);


                destParam.Add("ScriptType",
                    new[] {ScriptType.Recovery, ScriptType.Truncate, ScriptType.DisableConstraint});
                destParam.Add("SkipTables", request.Skip);
                _destinationDb = new DataSource(request, Position.Destination, destParam);
                ReportStatus("Initializing process...");
                if (request.CreateObjects)
                {
                    Parallel.Invoke(
                        () => _sourceDb.Initialize()
                        , () => _sourceDb.BuildObjects()
                        );
                    _destinationDb.Initialize();
                    ReportStatus("Executing schema.sql");
                    _destinationDb.ExecScript(Runtime.Path + "schema.sql");
                    ReportStatus("Executing tables.sql");
                    _destinationDb.ExecScript(Runtime.Path + "tables.sql");
                    _destinationDb.SetSourceScript(_sourceDb.GetSourceScript());
                }
                else
                {
                    Parallel.Invoke(
                        () => _sourceDb.Initialize()
                        , () => _destinationDb.Initialize()
                        );
                }
                if (request.CreateObjects)
                {
                }
                ReportStatus("Analyzing data...");

                var ds = _sourceDb.GetRowCount();
                var query = ds.Tables[0].AsEnumerable();
                var tables = query.Select(dr => dr.Field<string>("tablename")).ToArray();
                _eventArgs.TotalWork = (uint) tables.Length;
                var currentIndex = 1;
                var skip = _sourceDb.GetSkipList();
                if (!request.ResumeOnError && skip.Count > 0)
                {
                    foreach (var kvp in skip)
                    {
                        ReportStatus($"Error occured while processing  table [{kvp.Key}] --{kvp.Value} ...");
                    }
                    return null;
                }
                foreach (var table in tables)
                {
                    long rowsToBeStaged;
                    try
                    {
                        rowsToBeStaged =
                            query.Where(dr => dr.Field<string>("tablename").ToLower() == table.ToLower())
                                .Select(dr => dr.Field<long>("rowcount"))
                                .First();
                    }
                    catch
                    {
                        rowsToBeStaged = 0;
                    }
                    ReportStatus("Sending: [" + table + "]...");
                    if (rowsToBeStaged == 0 || skip.ContainsKey(table) ||
                        (request.Skip != null && request.Skip.Contains(table)))
                    {
                        if (skip.ContainsKey(table) || (request.Skip != null && request.Skip.Contains(table)))
                        {
                            ReportStatus("Skipping table [" + table + "] ...");
                        }
                        else
                        {
                            ReportStatus($"Transfering: [{table}] rows [{0}/{0}]...");
                        }
                        _eventArgs.CompletedWork += 1;
                        continue;
                    }
                    if (rowsToBeStaged > 0)
                    {
                        currentIndex = (int) Math.Floor(rowsToBeStaged/(decimal) request.BatchSize);
                        currentIndex = currentIndex > 0 ? currentIndex : 1;
                        if (rowsToBeStaged != currentIndex*request.BatchSize && rowsToBeStaged > request.BatchSize)
                            currentIndex++;
                    }

                    for (var i = 1; i <= currentIndex; i++)
                    {
                        try
                        {
                            var biLocalProvider = _sourceDb.GetProvider(table, i);
                            var biRemoteProvider = _destinationDb.GetProvider(table, i);

                            var agent = new BulkCopyAgent();
                            agent.ChangesSelected += agent_ChangesSelected;
                            agent.ChangesApplied += agent_ChangesApplied;
                            agent.LocalProvider = biLocalProvider;
                            agent.RemoteProvider = biRemoteProvider;

                            var writerows = agent.WriteToServer();

                            _changeTotal += Convert.ToUInt32(writerows);
                            _changeFailed += 0;
                            _changeApplied += Convert.ToUInt32(writerows);
                        }
                        catch (Exception ex)
                        {
                            ReportStatus(
                                $"Failed Transfering: [{table}] rows [{(rowsToBeStaged < i*request.BatchSize ? rowsToBeStaged : i*request.BatchSize)}/{rowsToBeStaged}]...\nError:{ex}");
                            if (request.ResumeOnError)
                            {
                                break;
                            }
                            {
                                throw;
                            }
                        }
                        ReportStatus(
                            $"Transfering: [{table}] rows [{(rowsToBeStaged < i*request.BatchSize ? rowsToBeStaged : i*request.BatchSize)}/{rowsToBeStaged}]...");
                    }
                    _eventArgs.CompletedWork += 1;
                }

                _eventArgs.Result = "";
                RaiseSessionProgress(_eventArgs);
                var status = new Dictionary<string, Dictionary<string, string>> {{"Skipped", skip}};
                _response = new BulkCopyResponse(_startTime, DateTime.Now, _changeTotal, _changeApplied, _changeFailed,
                    status);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                _sourceDb.Finalize(null);
                destParam.Add("EnableConstraintCheck", true);
                if (request.CreateObjects)
                {
                    ReportStatus("Executing indexes.sql");
                    _destinationDb.ExecScript(Runtime.Path + "indexes.sql");
                    ReportStatus("Executing views.sql");
                    _destinationDb.ExecScript(Runtime.Path + "views.sql");
                    ReportStatus("Executing procs.sql");
                    _destinationDb.ExecScript(Runtime.Path + "procs.sql");
                    ReportStatus("Executing fks.sql");
                    _destinationDb.ExecScript(Runtime.Path + "fks.sql");
                }
                _destinationDb.Finalize(destParam);
            }

            return _response;
        }

        private void ReportStatus(string msg)
        {
            _eventArgs.Result = msg;
            RaiseSessionProgress(_eventArgs);
        }

        private void agent_ChangesApplied(object sender, BulkInsertAppliedEventArgs e)
        {
            //Debug implementation only
        }

        private void agent_ChangesSelected(object sender, BulkInsertSelectedEventArgs e)
        {
            //Debug implementation only
        }

        private void RaiseSessionProgress(BulkCopyProgressEventArgs e)
        {
            BulkCopyProgress?.Invoke(this, e);
        }

        #endregion

        #region Logger

        private static void WriteLine(string msg)
        {
            Logger.Log(LogLevels.Info, msg);
        }

        //private void WriteLineFormat(string format, params object[] args)
        //{
        //    if (args != null)
        //    {
        //        Logger.Log(LogLevels.Info, string.Format(format, args));
        //    }
        //}

        #endregion

        #region IDisposable Members

        ~BulkCopyManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventArgs = null;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
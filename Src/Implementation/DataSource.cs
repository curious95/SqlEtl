using System.Collections.Generic;
using System.Data;
using SqlEtl.Entities;
using SqlEtl.Enums;
using SqlEtl.Interfaces;

namespace SqlEtl.Implementation
{
    internal class DataSource : IBulkCopy
    {
        #region ctor

        internal DataSource(BulkCopyRequest biRequest, Position position, Dictionary<string, object> param)
        {
            _request = biRequest;
            _param = param;
            _sessionData = new BulkCopySession
            {
                ResumeOnError = biRequest.ResumeOnError,
                ConnectionString =
                    position == Position.Source ? biRequest.LocalConnectionString : biRequest.RemoteConnectionString
            };
            _biProvider = new BulkCopyProvider(_sessionData);
        }

        #endregion

        #region memebr variable

        private readonly BulkCopySession _sessionData;
        private readonly BulkCopyProvider _biProvider;
        private readonly BulkCopyRequest _request;
        private readonly Dictionary<string, object> _param;

        #endregion

        #region IBulkCopy members

        public void Initialize()
        {
            _biProvider.Initialize(_param);
        }

        public BulkInsertProvider GetProvider(string bcTable, int index)
        {
            _sessionData.TableName = bcTable;
            _sessionData.BatchIndex = index;
            _sessionData.BatchSize = _request.BatchSize;
            _sessionData.ResumeOnError = _request.ResumeOnError;
            return _biProvider.CreateProvider(_sessionData);
        }

        public DataSet GetRowCount()
        {
            return _biProvider.GetRowCount();
        }

        public Dictionary<string, string> GetSkipList()
        {
            return _biProvider.GetSkipList();
        }

        public void BuildObjects()
        {
            _biProvider.BuildObjects();
        }

        public void ExecScript(string fileName)
        {
            _biProvider.ExecScript(fileName);
        }

        public void Finalize(Dictionary<string, object> param)
        {
            if (param != null)
            {
                _biProvider.Finalize(param);
            }
        }

        public Dictionary<string, object> GetSourceScript()
        {
            return _biProvider.GetSourceScript();
        }

        public void SetSourceScript(Dictionary<string, object> sourceScript)
        {
            _biProvider.SetSourceScript(sourceScript);
        }

        #endregion
    }
}
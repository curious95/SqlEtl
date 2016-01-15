using System;

namespace SqlEtl.Entities
{
    [Serializable]
    public abstract class BulkCopyBaseSession
    {
        public string TableName { get; set; }
        public int BatchIndex { get; set; }
        public int BatchSize { get; set; }
        public bool ResumeOnError { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace SqlEtl.Entities
{
    [Serializable]
    public sealed class BulkCopySession : BulkCopyBaseSession
    {
        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; set; }
        public Dictionary<string, string[]> CustomKeyDefinitions { get; set; } = new Dictionary<string, string[]>();
    }
}
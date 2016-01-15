using System;
using System.Collections.Generic;

namespace SqlEtl.Entities
{
    [Serializable]
    public sealed class BulkCopyRequest : BulkCopyBaseSession
    {
        public string LocalConnectionString { get; set; }
        public string RemoteConnectionString { get; set; }
        public string[] Skip { get; set; }
        public bool CreateObjects { get; set; }
        public int RetryCount { get; set; }
        public int RetryInterval { get; set; }
        public Dictionary<string, string[]> CustomKeyDefinitions { get; set; } = new Dictionary<string, string[]>();
    }
}
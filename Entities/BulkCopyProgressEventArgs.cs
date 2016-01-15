namespace SqlEtl.Entities
{
    public class BulkCopyProgressEventArgs
    {
        public uint TotalWork { get; set; }
        public uint CompletedWork { get; set; }
        public string Result { get; set; }
    }
}
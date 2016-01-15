using System.Data;

namespace SqlEtl.Entities
{
    public class ScopeColumn
    {
        public string Name { get; set; }
        public SqlDbType Type { get; set; }
        public int Length { get; set; }
        public bool Primary { get; set; }
    }
}
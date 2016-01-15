namespace SqlEtl.Entities
{
    public class ScopeTable
    {
        public ScopeColumnCollection Columns { get; set; } = new ScopeColumnCollection();
        public string Name { get; set; } = string.Empty;
    }
}
namespace DataModelMigration.Model.ClassModel
{
    public class SourceLookupValues : LookupVales
    {
        public int Id { get; set; }
    }

    public class LookupVales
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}

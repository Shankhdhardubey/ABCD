namespace DataModelMigration.Model.ClassModel
{
    public class MigrationConfiguration
    {
        public int Id { get; set; }

        public string Source_Server_Name { get; set; }

        public string Source_Database_Name { get; set; }

        public string Source_UserName { get; set; }

        public string Source_Password { get; set; }

        public string Target_Server_Name { get; set; }

        public string Target_Database_Name { get; set; }

        public string Target_UserName { get; set; }

        public string Target_Password { get; set; }
    }
}

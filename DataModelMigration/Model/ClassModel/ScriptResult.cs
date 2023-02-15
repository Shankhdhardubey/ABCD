using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelMigration.Model.DataModel
{
    [Table("ScriptResult")]
    public class ScriptResult
    {
        [Key]
        public string Script_Log { get; set; }
    }

    public class OutputTable
    {
        [Key]
        public string table_name { get; set; }
    }
}

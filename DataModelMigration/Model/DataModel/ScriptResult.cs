using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMigration.Model.DataModel
{
    [Table("ScriptResult")]
    public class ScriptResult
    {
        [Key]
        public string Script_Log { get; set; }
    }
}

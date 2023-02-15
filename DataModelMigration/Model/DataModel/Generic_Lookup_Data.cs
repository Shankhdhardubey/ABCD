using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModelMigration.Model.DataModel
{
    [Table("Generic_Lookup_Data")]
   public  class Generic_Lookup_Data
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Generic_Lookup_Data_id { get; set; }
        public string Source_Code { get; set; }
        public string Source_Description { get; set; }
        public string Target_Code { get; set; }
        public string Target_Description { get; set; }
        public string LookUp_Name { get; set; }

    }
}

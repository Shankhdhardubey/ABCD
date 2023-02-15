using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataMigration.Model.DataModel
{
    [Table("UserDetailsRequest")]
    public class UserDetailsRequest
    {
        [Key]
        public short UserId { get; set; }

        public string UserName { get; set; }

        public string CompanyName { get; set; }

        public string FaxNumber { get; set; }

        public string ExtensionNumber { get; set; }

        public string Title { get; set; }

        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public string MobileNumber { get; set; }

        public string TelephoneNumber { get; set; }
    }
}

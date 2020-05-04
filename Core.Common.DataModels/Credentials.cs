using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public class Credentials
    {
        [Required]
        [EmailAddress]
        [DisplayName("Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(256, ErrorMessage ="The {0} must be at least {2} characters long.", MinimumLength =12)]
        [DataType("Password")]
        [DisplayName("Password")]
        public string Password { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public class RefreshTokenCredentials
    {
        [Required]
        [DisplayName("UserName")]
        public string UserName { get; set; }

        [Required]
        [DisplayName("RefreshToken")]
        public string RefreshToken { get; set; }
    }
}

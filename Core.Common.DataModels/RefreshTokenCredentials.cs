using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public record RefreshTokenCredentials
    {
        [Required]
        [DisplayName("UserName")]
        public string UserName { get; init; } = string.Empty;

        [Required]
        [DisplayName("RefreshToken")]
        public string RefreshToken { get; init; } = string.Empty;
    }
}

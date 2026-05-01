using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public class JWTSettings
    {
        [Required]
        [MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters (256 bits) for HMAC-SHA256.")]
        public string SecretKey { get; set; }

        [Required]
        public string Issuer { get; set; }

        [Required]
        public string Audience { get; set; }

        public int ExpiryMinutes { get; set; }
        public int RefreshTokenExpiryMinutes { get; set; }
    }
}
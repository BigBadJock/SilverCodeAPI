using System;

namespace Core.Common.DataModels
{
    public class RefreshToken: BaseModel
    {
        public string UserId { get; set; }
        public Guid Token { get; set; }
        public DateTime Expiry { get; set; }

    }
}

using Core.Common.DataModels.Interfaces;

namespace Core.Common.DataModels
{
    public class UserClaim: BaseModel
    {
        public string UserId { get; set; }
        public int CustomClaimId { get; set; }
    }
}

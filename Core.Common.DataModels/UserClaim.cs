using Core.Common.DataModels.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Common.DataModels
{
    public class UserClaim: BaseModel
    {
        public string UserId { get; set; }
        [ForeignKey("CustomClaimType")]
        public int CustomClaimId { get; set; }
    }
}

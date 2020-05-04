using Core.Common.DataModels.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Core.Common.DataModels
{
    public abstract class BaseUser : IdentityUser, IBaseUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

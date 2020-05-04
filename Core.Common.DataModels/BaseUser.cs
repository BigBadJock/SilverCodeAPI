using Microsoft.AspNetCore.Identity;
using Core.Common.DataModels.Interfaces;


namespace Core.Common.DataModels
{
    public abstract class BaseUser : IdentityUser, IBaseUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

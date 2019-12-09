using Core.Common.Contracts;
using System.ComponentModel.DataAnnotations;

namespace Core.Common
{
    public abstract class BaseLookupModel : BaseModel, ILookupModel
    {
        [Required]
        public string Name { get ; set ; }
    }
}

using Core.Common.DataModels.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public abstract class BaseLookupModel : BaseModel, ILookupModel
    {
        [Required]
        public string Name { get ; set ; }
    }
}

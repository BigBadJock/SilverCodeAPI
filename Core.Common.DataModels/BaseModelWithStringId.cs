using Core.Common.DataModels.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public abstract class BaseModelWithStringId : BaseModel, IModelWithStringId
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        public BaseModelWithStringId() : base()
        {

        }

    }
}

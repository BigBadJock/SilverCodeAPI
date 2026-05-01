using Core.Common.DataModels.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Common.DataModels
{
    public abstract class BaseModelWithGuidId : BaseModel, IModelWithGuidId
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public BaseModelWithGuidId() : base()
        {
        }
    }
}

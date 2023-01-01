using Core.Common.DataModels.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Common.DataModels
{
    public class BaseModelWithGuidId : BaseModel, IModelWithGuidId
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        /// <example>1</example>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public BaseModelWithGuidId() : base()
        {
        }
    }
}

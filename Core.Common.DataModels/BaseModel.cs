using Core.Common.DataModels.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Common.DataModels
{
    public abstract class BaseModel : IModel
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        /// <example>1</example>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Delete flag
        /// </summary>
        /// <example>false</example>
        [Editable(false)]
        public bool IsDeleted { get; set; } 

        /// <summary>
        /// Date & Time entity created
        /// </summary>
        [Editable(false)]
        public DateTime Created { get; set; }

        /// <summary>
        /// Date & Time entity last updated
        /// </summary>
        [Editable(false)]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// user name of last updater
        /// </summary>
        [Editable(false)]
        public string LastUpdatedBy { get; set; }

        public BaseModel()
        {
            Id = Guid.NewGuid();
            IsDeleted = false;
            Created = DateTime.Now;
            LastUpdated = DateTime.Now;
        }
    }
}

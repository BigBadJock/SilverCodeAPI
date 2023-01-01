using Core.Common.DataModels.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public abstract class BaseModel : IModel
    {
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
        /// user name of creator 
        /// </summary>
        [Editable(false)]
        public string CreatedBy { get; set; }

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
            IsDeleted = false;
            Created = DateTime.Now;
            LastUpdated = DateTime.Now;
        }
    }
}

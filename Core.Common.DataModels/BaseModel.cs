using Core.Common.DataModels.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.DataModels
{
    public abstract class BaseModel : IModel
    {
        [Key]
        public int Id { get; set; }
        [Editable(false)]
        public bool IsDeleted { get; set; }
        [Editable(false)]
        public DateTime Created { get; set; }
        [Editable(false)]
        public DateTime LastUpdated { get; set; }
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

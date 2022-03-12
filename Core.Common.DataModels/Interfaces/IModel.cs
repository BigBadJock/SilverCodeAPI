using System;

namespace Core.Common.DataModels.Interfaces
{
    public interface IModel
    {
         Guid Id { get; set; }
         bool IsDeleted { get; set; }
         DateTime Created { get; set; }
         DateTime LastUpdated { get; set; }
         string LastUpdatedBy { get; set; }
    }
}

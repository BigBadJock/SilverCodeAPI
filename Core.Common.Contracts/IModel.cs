using System;

namespace Core.Common.Contracts
{
    public interface IModel
    {
         int Id { get; set; }
         bool IsDeleted { get; set; }
         DateTime Created { get; set; }
         DateTime LastUpdated { get; set; }
         string LastUpdatedBy { get; set; }
    }
}

using System;

namespace Core.Common.DataModels.Interfaces
{
    public interface IModel
    {
        bool IsDeleted { get; set; }
        DateTime Created { get; set; }
        string CreatedBy { get; set; }
        DateTime? LastUpdated { get; set; }
        string? LastUpdatedBy { get; set; }
    }
}

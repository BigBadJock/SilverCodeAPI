using System;

namespace Core.Common.DataModels.Interfaces
{
    public interface IModelWithGuidId
    {
        Guid Id { get; set; }
    }
}

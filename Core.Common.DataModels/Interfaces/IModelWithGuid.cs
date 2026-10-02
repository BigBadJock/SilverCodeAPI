using System;

namespace Core.Common.DataModels.Interfaces
{
    public interface IModelWithGuidId : IModel
    {
        Guid Id { get; set; }
    }
}

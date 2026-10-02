namespace Core.Common.DataModels.Interfaces
{
    public interface IModelWithStringId : IModel
    {
        string Id { get; set; }
    }
}

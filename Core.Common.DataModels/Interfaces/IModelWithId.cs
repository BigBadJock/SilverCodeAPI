namespace Core.Common.DataModels.Interfaces
{
    public interface IModelWithIntId : IModel
    {
        int Id { get; set; }
    }
}

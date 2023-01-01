namespace Core.Common.DataModels.Interfaces
{
    public interface ILookupModel : IModelWithIntId
    {
        string Name { get; set; }
    }
}

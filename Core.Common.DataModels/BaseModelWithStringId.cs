using Core.Common.DataModels.Interfaces;

namespace Core.Common.DataModels
{
    public class BaseModelWithStringId : BaseModel, IModelWithStringId
    {
        public string Id { get; set; }

        public BaseModelWithStringId() : base()
        {

        }

    }
}

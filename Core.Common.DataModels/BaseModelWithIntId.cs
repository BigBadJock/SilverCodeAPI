using Core.Common.DataModels.Interfaces;

namespace Core.Common.DataModels
{
    public abstract class BaseModelWithIntId : BaseModel, IModelWithIntId
    {
        public int Id { get; set; }

        public BaseModelWithIntId() : base()
        {

        }

    }
}

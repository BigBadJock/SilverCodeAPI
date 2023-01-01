using Core.Common.DataModels.Interfaces;

namespace Core.Common.DataModels
{
    public class BaseModelWithIntId : BaseModel, IModelWithIntId
    {
        public int Id { get; set; }

        public BaseModelWithIntId() : base()
        {

        }

    }
}

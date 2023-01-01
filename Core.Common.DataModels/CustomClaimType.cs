using Core.Common.DataModels.Interfaces;

namespace Core.Common.DataModels
{
    public class CustomClaimType : BaseModelWithIntId, ILookupModel
    {
        public string Name { get; set; }
    }
}

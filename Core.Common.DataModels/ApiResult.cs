using System.Collections.Generic;

namespace Core.Common.DataModels
{
    public class ApiResult<T>
    {
        public IEnumerable<T> Data { get; set; }

        public Pagination? Pagination { get; set; }
    }
}

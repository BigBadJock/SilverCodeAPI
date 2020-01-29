using System.Collections.Generic;

namespace Core.Common
{
    public class ApiResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public Pagination? Pagination { get; set; }
    }
}

using System.Collections.Generic;

namespace Core.Common.DataModels
{
    public record ApiResult<T>
    {
        public IEnumerable<T> Data { get; init; } = [];
        public Pagination? Pagination { get; init; }
    }
}

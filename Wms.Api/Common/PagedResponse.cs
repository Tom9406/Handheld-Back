namespace Wms.Api.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}

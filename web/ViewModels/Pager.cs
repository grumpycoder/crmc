using System.Collections.Generic;

namespace web.ViewModels
{
    public class Pager<T>
    {
        public int? Page { get; set; } = 0;
        public int? PageSize { get; set; } = 20;
        public bool AllRecords { get; set; } = false;
        public string OrderBy { get; set; }
        public string OrderDirection { get; set; }

        public int TotalCount { get; set; }
        public int Count { get; set; }

        public int FilteredCount { get; set; }
        public int TotalPages { get; set; }

        public IList<T> Items { get; set; }
    }
}
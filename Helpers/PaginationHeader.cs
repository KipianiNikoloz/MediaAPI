namespace API.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int itemsPerPage, int totalPages, int totalCount)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalPages = totalPages;
            TotalCount = totalCount;
        }
        
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
namespace OnlineBookstore.ViewModels
{
    // Lightweight view model for listing publisher
    public class PublisherListItemVm
    {
        public int PublisherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BookCount { get; set; }
    }

    // Detailed view model for a single publisher (includes books)
    public class PublisherDetailsVm
    {
        public int PublisherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public List<BookBriefVm> Books { get; set; } = new();
    }

    // Compact book representation for use inside PublisherDetailsVm
    public class BookBriefVm
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

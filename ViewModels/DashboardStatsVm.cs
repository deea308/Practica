namespace OnlineBookstore.ViewModels
{
    // Holds overall counts of key entities for admin dashboards
    public class DashboardStatsVm
    {
        public int BooksCount { get; set; }
        public int AuthorsCount { get; set; }
        public int GenresCount { get; set; }
        public int PublishersCount { get; set; }
        public int ReviewsCount { get; set; }
        public int UsersCount { get; set; }
    }
}

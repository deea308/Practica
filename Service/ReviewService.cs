using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineBookstore.Service
{
    // Provides high-level operations for Reviews
    public class ReviewService
    {
        private readonly ReviewRepository _reviews;

        public ReviewService(ReviewRepository reviews)
        {
            _reviews = reviews;
        }

        public Task<IEnumerable<Review>> GetAllReviewsAsync()
            => _reviews.GetAllAsync();          

        public Task<Review?> GetReviewAsync(int id)
            => _reviews.GetByIdAsync(id);       

        public Task AddReviewAsync(Review review)
            => _reviews.AddAsync(review);

        public Task UpdateReviewAsync(Review review)
            => _reviews.UpdateAsync(review);

        public Task DeleteReviewAsync(int id)
            => _reviews.DeleteAsync(id);
    }
}

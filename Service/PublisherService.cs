using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineBookstore.Service
{
    // Provides high-level operations for Publishers
    public class PublisherService
    {
        private readonly PublisherRepository _publishers;

        public PublisherService(PublisherRepository publishers)
        {
            _publishers = publishers;
        }

        public Task<IEnumerable<Publisher>> GetAllPublishersAsync()
            => _publishers.GetAllAsync();

        public Task<Publisher?> GetPublisherAsync(int id)
            => _publishers.GetByIdAsync(id);

        public Task AddPublisherAsync(Publisher publisher)
            => _publishers.AddAsync(publisher);

        public Task UpdatePublisherAsync(Publisher publisher)
            => _publishers.UpdateAsync(publisher);

        public Task DeletePublisherAsync(int id)
            => _publishers.DeleteAsync(id);

    }
}

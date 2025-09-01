using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineBookstore.Service
{
    // Provides high-level operations for Users 
    public class UserService
    {
        private readonly UserRepository _users;

        public UserService(UserRepository users)
        {
            _users = users;
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
            => _users.GetAllAsync();          

        public Task<User?> GetUserAsync(int id)
            => _users.GetByIdAsync(id);      

        public Task AddUserAsync(User user)
            => _users.AddAsync(user);

        public Task UpdateUserAsync(User user)
            => _users.UpdateAsync(user);

        public Task DeleteUserAsync(int id)
            => _users.DeleteAsync(id);
    }
}

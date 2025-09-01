using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{
    /// Represents an application user/customer who can write reviews and manage an account.
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;
        public string PasswordHash { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }

        public List<Review> Reviews { get; set; } = new();
    }

}

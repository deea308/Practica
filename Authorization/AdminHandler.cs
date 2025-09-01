
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookStore_Data.Data;   
using System.Security.Claims;

namespace OnlineBookstore.Authorization
{
    // Custom authorization handler that checks if a user is an Admin
    public class AdminHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly BookstoreContext _ctx;
        private readonly IMemoryCache _cache;

        public AdminHandler(BookstoreContext ctx, IMemoryCache cache)
        {
            _ctx = ctx;
            _cache = cache;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return;

            var cacheKey = $"is-admin:{userId}";
            if (!_cache.TryGetValue(cacheKey, out bool isAdmin))
            {
                isAdmin = await _ctx.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => u.IsAdmin)   
                    .FirstOrDefaultAsync();

                _cache.Set(cacheKey, isAdmin, TimeSpan.FromSeconds(60));
            }

            if (isAdmin)
                context.Succeed(requirement);
        }
    }
}

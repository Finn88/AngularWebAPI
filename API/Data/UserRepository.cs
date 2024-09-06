using API.Dto;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
    {
        public async Task<MemberDto?> GetMemberByNameAsync(string name)
        {
            return await context.Users
                .Where(x => x.NormalizedUserName == name.ToUpper())
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }
         
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            if (userParams.CurrentUsername is null) return null;

            var query = context.Users
                .AsQueryable()
                .Where(c => c.NormalizedUserName != userParams.CurrentUsername.ToUpper());

            if (!string.IsNullOrEmpty(userParams.Gender))
                query = query.Where(c => c.Gender == userParams.Gender);

            var minDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            query = query.Where(c => c.DateOfBirth >= minDate && c.DateOfBirth <= maxDate);
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
               _ => query.OrderByDescending(x => x.LastActive),
            };

            var members = query.ProjectTo<MemberDto>(mapper.ConfigurationProvider);
            return await PagedList<MemberDto>.CreateAsync(members, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUser?> GetUserByNameAsync(string name)
        {
            return await context.Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.NormalizedUserName == name.ToUpper());
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await context.Users
                .Include(x => x.Photos)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }


        public void DeleteUser(int userId)
        {
            var user = context.Users.Include(u => u.LikedUsers).Include(u => u.LikedByUsers).FirstOrDefault(u => u.Id == userId);
            if (user is null) { return; }

            context.Likes.RemoveRange(user.LikedUsers);
            context.Likes.RemoveRange(user.LikedByUsers);

            context.Users.Remove(user);
            context.SaveChangesAsync();
        }
    }
}

using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DataAccessLayer.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public ApplicationUser()
        {
            Orders = new HashSet<Order>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool AccountIsEnabled { get; set; }

        public virtual LockAccountReason LockAccountReason { get; set; }
        public virtual Address Address { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }

    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }

    public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
    {
        public CustomRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }
    
    public interface ICustomUserStore<TUser, in TKey> : IUserStore<TUser, TKey>, IDisposable where TUser : class, IUser<TKey>
    {
        Task<Tuple<IEnumerable<ApplicationUser>, int>> PageAllAsync(int? pageNumber, int? pageSize, string searchByFirstName, string searchByLastName, string searchByEmail);
        Task<Address> GetUserAddressAsync(int userId);
        Task LockUserAccountAsync(int userId, string lockReason);
        Task UnlockUserAccountAsync(int userId);
        Task UpdateUserAddressAsync(Address address);
        Task<bool> IsUserAddressSetAsync(int userId);
    }

    public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int,
        CustomUserLogin, CustomUserRole, CustomUserClaim>, ICustomUserStore<ApplicationUser,int>
    {
        private ApplicationDbContext context;
        public CustomUserStore(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<Tuple<IEnumerable<ApplicationUser>, int>> PageAllAsync(int? pageNumber, int? pageSize, string searchByFirstName, string searchByLastName, string searchByEmail)
        {
            IQueryable<ApplicationUser> query = context.Users;

            if (!string.IsNullOrEmpty(searchByFirstName))
                query = query.Where(u => u.FirstName.ToLower().Contains(searchByFirstName.ToLower()));

            if (!string.IsNullOrEmpty(searchByLastName))
                query = query.Where(u => u.LastName.ToLower().Contains(searchByLastName.ToLower()));

            if (!string.IsNullOrEmpty(searchByEmail))
                query = query.Where(u => u.Email.ToLower().Contains(searchByEmail.ToLower()));

            int usersCount = query.Count();

            if (pageNumber != null && pageSize != null)
            {
                query = query.OrderBy(u => u.Id);
                query = query.Skip(((int)pageNumber - 1) * (int)pageSize);
                query = query.Take((int)pageSize);
            }
            IEnumerable<ApplicationUser> users = await query.ToListAsync();
            return Tuple.Create(users, usersCount);
        }

        public async Task<Address> GetUserAddressAsync(int userId)
        {
            return await context.Address.FindAsync(userId);
        }

        public async Task LockUserAccountAsync(int userId, string lockReason)
        {
            ApplicationUser user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            user.AccountIsEnabled = false;
            context.Entry(user).State = EntityState.Modified;

            context.LockAccountReasons.Add(new LockAccountReason { UserId = userId, LockReason = lockReason });

            await context.SaveChangesAsync();
        }

        public async Task UnlockUserAccountAsync(int userId)
        {
            ApplicationUser user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            user.AccountIsEnabled = true;
            context.Entry(user).State = EntityState.Modified;

            LockAccountReason reason = await context.LockAccountReasons.FindAsync(userId);
            context.LockAccountReasons.Remove(reason);

            await context.SaveChangesAsync();
        }

        public async Task UpdateUserAddressAsync(Address address)
        {
            Address currentAddress = await context.Address.FindAsync(address.UserId);
            if(currentAddress != null)
            {
                currentAddress.City = address.City;
                currentAddress.Street = address.Street;
                currentAddress.HouseNumber = address.HouseNumber;
                currentAddress.ZipCode = address.ZipCode;
                context.Entry(currentAddress).State = EntityState.Modified;
            }
            else
            {
                context.Address.Add(address);
            }
            await context.SaveChangesAsync();
        }

        public async Task<bool> IsUserAddressSetAsync(int userId)
        {
            return await context.Address.AnyAsync(a => a.UserId == userId);
        }
    }
}
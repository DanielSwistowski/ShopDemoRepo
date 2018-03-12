using DataAccessLayer.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service_Layer.Services
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        private static IDataProtectionProvider dataProtectionProvider;
        public ICustomUserStore<ApplicationUser, int> customStore;
        public ApplicationUserManager(ICustomUserStore<ApplicationUser, int> store)
            : base(store)
        {
            customStore = store;
            // Configure validation logic for usernames
            this.UserValidator = new UserValidator<ApplicationUser, int>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            this.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                //RequireLowercase = true,
                //RequireUppercase = true,
            };

            // Configure user lockout defaults
            this.UserLockoutEnabledByDefault = true;
            this.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            this.MaxFailedAccessAttemptsBeforeLockout = 5;
            

            if (dataProtectionProvider != null)
            {
                IDataProtector dataProtector = dataProtectionProvider.Create("ASP.NET Identity");
                this.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, int>(dataProtector);
            }
        }

        public static void SetDataProtectionProvider(IDataProtectionProvider provider)
        {
            dataProtectionProvider = provider;
        }

        public virtual async Task<Tuple<IEnumerable<ApplicationUser>, int>> PageAllAsync(int? pageNumber, int? pageSize, string searchByFirstName, string searchByLastName, string searchByEmail)
        {
            return await customStore.PageAllAsync(pageNumber, pageSize, searchByFirstName, searchByLastName, searchByEmail);
        }

        public virtual async Task<Address> GetUserAddressAsync(int userId)
        {
            return await customStore.GetUserAddressAsync(userId);
        }

        public virtual async Task LockUserAccountAsync(int userId, string lockReason)
        {
            await customStore.LockUserAccountAsync(userId, lockReason);
        }

        public virtual async Task UnlockUserAccountAsync(int userId)
        {
            await customStore.UnlockUserAccountAsync(userId);
        }

        public virtual async Task UpdateUserAddressAsync(Address address)
        {
            await customStore.UpdateUserAddressAsync(address);
        }

        public virtual async Task<bool> IsUserAddressSetAsync(int userId)
        {
            return await customStore.IsUserAddressSetAsync(userId);
        }
    }
}
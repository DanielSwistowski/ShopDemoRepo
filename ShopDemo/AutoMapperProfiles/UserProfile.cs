using AutoMapper;
using DataAccessLayer.Models;
using ShopDemo.ViewModels;

namespace ShopDemo.AutoMapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UsersListViewModel>()
                .ForMember(d => d.AccountStatus, o => o.ResolveUsing<ResolveAccountStatus>());

            CreateMap<ApplicationUser, UserDetailsVewModel>()
                .ForMember(d => d.Roles, o => o.Ignore());

            CreateMap<Address, UserAddressViewModel>();

            CreateMap<ApplicationUser, LockUserAccountViewModel>();

            CreateMap<ApplicationUser, UnlockUserAccountViewModel>();

            CreateMap<ApplicationUser, UserPersonalDataViewModel>()
                .ForMember(d => d.Roles, o => o.Ignore());

            CreateMap<ApplicationUser, EditPersonalDataViewModel>();

            CreateMap<Address, EditAddressViewModel>().ReverseMap();

            CreateMap<ApplicationUser, UserBaseDataViewModel>()
                .ForMember(d => d.AccountStatus, o => o.ResolveUsing<ResolveAccountStatus>());

            CreateMap<ApplicationUser, CustomerDataViewModel>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
                .ForMember(d => d.Street, o => o.MapFrom(s => s.Address.Street + " " + s.Address.HouseNumber))
                .ForMember(d => d.City, o => o.MapFrom(s => s.Address.ZipCode + " " + s.Address.City));
        }
    }

    public class ResolveAccountStatus : IValueResolver<ApplicationUser, object, bool>
    {
        public bool Resolve(ApplicationUser source, object destination, bool destMember, ResolutionContext context)
        {
            bool accountIsEnabled = true;

            if (source.LockoutEndDateUtc != null)
                accountIsEnabled = false;

            if (!source.AccountIsEnabled)
                accountIsEnabled = false;

            return accountIsEnabled;
        }
    }
}
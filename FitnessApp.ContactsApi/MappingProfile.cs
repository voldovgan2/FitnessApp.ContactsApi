using AutoMapper;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SearchUserEntity, UserEntity>()
        .ForMember(e => e.FollowersCount, m => m.Ignore())
            .ForMember(e => e.CategoryDate, m => m.Ignore());

        CreateMap<PagedDataModel<SearchUserEntity>, PagedDataModel<UserModel>>();
        CreateMap<UserModel, UserEntity>();
        CreateMap<UserEntity, FirstCharSearchUserEntity>();
        CreateMap<FirstCharSearchUserEntity, UserModel>();
    }
}

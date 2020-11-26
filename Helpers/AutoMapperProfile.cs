
namespace HeroGame.Helpers
{
    using AutoMapper;
    using HeroGame.Entities;
    using HeroGame.Models;

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, UserModel>();
            CreateMap<RegisterModel, Account>();
            CreateMap<UpdateModel, Account>();
            CreateMap<RegisterHeroes, Hero>();
        }
    }
}

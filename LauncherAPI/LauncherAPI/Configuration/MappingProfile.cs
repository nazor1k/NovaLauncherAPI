
using AutoMapper;
using BLL;
using DomainCore;
using JWTToken.Model;

namespace JWTToken.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            var tokenService = ServiceLocator.ServiceProvider.GetRequiredService<TokenService>();

            CreateMap<RegisterDTO, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.Ignore());

        }
    }
}

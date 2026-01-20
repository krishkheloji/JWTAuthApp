using AutoMapper;
using JWTPractice.DTOs;
using JWTPractice.Models;

namespace JWTPractice.Mapping
{

    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

        }
    }

}

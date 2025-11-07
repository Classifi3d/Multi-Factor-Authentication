using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;
using MFAWebApplication.Enteties;

namespace MFAWebApplication.DTOs;

public class MapperConfiguration
{
    public static Mapper InitializeAutomapper()
    {
        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDTO>().ReverseMap();
            cfg.CreateMap<User, UserCreatedEvent>().ReverseMap();
            cfg.CreateMap<UserCreatedEvent, UserReadModel>()
                .ForMember(dest => dest.Id, 
                            opt => opt.MapFrom(src => src.Id.ToString()));
        }
        );

        var mapper = new Mapper(config);
        return mapper;
    }
}

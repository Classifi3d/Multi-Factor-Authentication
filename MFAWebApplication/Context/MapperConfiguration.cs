using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using AutoMapper;

namespace AuthenticationWebApplication.Context
{
    public class MapperConfiguration
    {
        public static Mapper InitializeAutomapper()
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDTO>().ReverseMap();
            }
            );

            var mapper = new Mapper(config);
            return mapper;
        }
    }
}

using DbManager.Data.Nodes;
using DbManager.Data;
using AutoMapper;
using DbManager.Data.DTOs;

namespace DbManager.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UserOutDTO>();
            CreateMap<Client, UserOutDTO>();
            CreateMap<Admin, UserOutDTO>();
            CreateMap<KitchenWorker, UserOutDTO>();


        }
    }
}

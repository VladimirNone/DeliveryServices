using DbManager.Data.Nodes;
using DbManager.Data;
using AutoMapper;
using DbManager.Data.DTOs;
using DbManager.Data.Relations;
using Neo4jClient.Extensions;

namespace DbManager.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, ProfileUserOutDTO>();
            CreateMap<Client, ProfileUserOutDTO>();
            CreateMap<Admin, ProfileUserOutDTO>();
            CreateMap<KitchenWorker, ProfileUserOutDTO>();

            CreateMap<User, UserForAdminOutDTO>();
            CreateMap<List<string>, UserForAdminOutDTO>()
                .ForMember(h => h.Roles, opt => opt.MapFrom(src => string.Join(", ", src)));

            CreateMap<KitchenWorker, KitchenWorkerOutDTO>();

            CreateMap<OrderState, OrderStateItemOutDTO>()
                .ForMember(h=>h.OrderStateId, (o) => o.MapFrom(src=>src.Id));

            CreateMap<HasOrderState, OrderStateItemOutDTO>()
                .BeforeMap((h,k) => h.NodeTo = OrderState.OrderStatesFromDb.FirstOrDefault(s=>s.Id == h.NodeToId))
                .ForMember(h => h.OrderStateId, (o) => o.MapFrom(src => src.NodeToId))
                .ForMember(h => h.NumberOfStage, (o) => o.MapFrom(src => ((OrderState)src.NodeTo).NumberOfStage))
                .ForMember(h => h.NameOfState, (o) => o.MapFrom(src => ((OrderState)src.NodeTo).NameOfState))
                .ForMember(h => h.DescriptionForClient, (o) => o.MapFrom(src => ((OrderState)src.NodeTo).DescriptionForClient));

            CreateMap<Order, OrderOutDTO>()
                .ForMember(dest => dest.Story, opt => opt.MapFrom(src => src.Story));

            CreateMap<ReviewedBy, OrderOutDTO>()
                .ForMember(h => h.Review, (o) => o.MapFrom(src => src.Review))
                .ForMember(h => h.ClientRating, (o) => o.MapFrom(src => src.ClientRating));

            CreateMap<Dish, ManipulateDishDataInDTO>();
            CreateMap<ManipulateDishDataInDTO, Dish>()
                .ForMember(h=>h.Id, o => o.Ignore());
        }
    }
}

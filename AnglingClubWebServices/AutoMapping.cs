using AnglingClubWebServices.DTOs;
using AnglingClubWebServices.Models;
using AutoMapper;

namespace AnglingClubWebServices
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<ClubEvent, ClubEventInputDto>().ReverseMap();
            CreateMap<MatchResult, MatchResultInputDto>().ReverseMap();
            CreateMap<MatchResult, MatchResultOutputDto>().ReverseMap();
            CreateMap<PaymentMetaData, OrderDetailDto>().ReverseMap();
            CreateMap<Order, OrderDetailDto>().ReverseMap();
        }
    }
}

using AnglingClubShared.Entities;
using AutoMapper;

namespace AnglingClubWebsite
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<DocumentMeta, DocumentMetaDTO>().ReverseMap();
        }
    }
}

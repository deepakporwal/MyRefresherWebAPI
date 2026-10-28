using AutoMapper;
using MyRefresherWebAPI.Models.Domain;
using MyRefresherWebAPI.Models.DTO;

namespace MyRefresherWebAPI.Profiles;

public sealed class RegionProfile : Profile
{
    public RegionProfile()
    {
        CreateMap<Region, RegionDTO>();
    }
}

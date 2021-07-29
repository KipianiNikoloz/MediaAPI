using System;
using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MembersDto>()
                .ForMember(dest => dest.PhotoUrl, 
                    opt => 
                        opt.MapFrom(src => 
                            src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age,
                    opt => 
                        opt.MapFrom(src => 
                            src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MembersUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderUrl,
                    opt =>
                        opt.MapFrom(src =>
                            src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.RecipientUrl,
                opt =>
                    opt.MapFrom(src =>
                        src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
        }
    }
}
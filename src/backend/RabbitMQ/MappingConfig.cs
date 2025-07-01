using AutoMapper;
using Data.Entities;
using Data.ViewModels;

namespace Services
{
    internal class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<ClinicViewModel, Clinic>()
                .ForMember(dest => dest.CreateBy, src => src.Ignore())
                .ForMember(dest => dest.CreateDate, src => src.Ignore())
                .ForMember(dest => dest.UpdateBy, src => src.Ignore())
                .ForMember(dest => dest.UpdateDate, src => src.Ignore())
                .ForMember(dest => dest.Id, src => src.MapFrom(opt => opt.ClinicId))
                .ReverseMap();

            CreateMap<DoctorViewModel, Doctor>()
                .ForMember(dest => dest.CreateBy, src => src.Ignore())
                .ForMember(dest => dest.CreateDate, src => src.Ignore())
                .ForMember(dest => dest.UpdateBy, src => src.Ignore())
                .ForMember(dest => dest.UpdateDate, src => src.Ignore())
                .ForMember(dest => dest.Id, src => src.MapFrom(opt => opt.DoctorId))
                .ReverseMap();

            CreateMap<AppointmentViewModel, Appointment>()
                .ForMember(dest => dest.CreateBy, src => src.Ignore())
                .ForMember(dest => dest.CreateDate, src => src.Ignore())
                .ForMember(dest => dest.UpdateBy, src => src.Ignore())
                .ForMember(dest => dest.UpdateDate, src => src.Ignore())
                .ForMember(dest => dest.Id, src => src.MapFrom(opt => opt.AppointmentId))
                .ReverseMap();
        }
    }
}

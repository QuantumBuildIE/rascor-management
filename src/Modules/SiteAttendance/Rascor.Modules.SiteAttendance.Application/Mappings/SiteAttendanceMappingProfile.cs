using AutoMapper;
using Rascor.Modules.SiteAttendance.Application.DTOs;
using Rascor.Modules.SiteAttendance.Domain.Entities;

namespace Rascor.Modules.SiteAttendance.Application.Mappings;

public class SiteAttendanceMappingProfile : Profile
{
    public SiteAttendanceMappingProfile()
    {
        // AttendanceEvent mappings
        CreateMap<AttendanceEvent, AttendanceEventDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty))
            .ForMember(dest => dest.SiteName, opt => opt.MapFrom(src =>
                src.Site != null ? src.Site.SiteName : string.Empty))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.TriggerMethod, opt => opt.MapFrom(src => src.TriggerMethod.ToString()));

        // AttendanceSummary mappings
        CreateMap<AttendanceSummary, AttendanceSummaryDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty))
            .ForMember(dest => dest.SiteName, opt => opt.MapFrom(src =>
                src.Site != null ? src.Site.SiteName : string.Empty))
            .ForMember(dest => dest.TimeOnSiteHours, opt => opt.MapFrom(src => src.ActualHours))
            .ForMember(dest => dest.VarianceHours, opt => opt.MapFrom(src => src.VarianceHours))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // SitePhotoAttendance mappings
        CreateMap<SitePhotoAttendance, SitePhotoAttendanceDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty))
            .ForMember(dest => dest.SiteName, opt => opt.MapFrom(src =>
                src.Site != null ? src.Site.SiteName : string.Empty));

        // AttendanceSettings mappings
        CreateMap<AttendanceSettings, AttendanceSettingsDto>();
        CreateMap<UpdateAttendanceSettingsRequest, AttendanceSettings>();

        // DeviceRegistration mappings
        CreateMap<DeviceRegistration, DeviceRegistrationDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : null));

        // BankHoliday mappings
        CreateMap<BankHoliday, BankHolidayDto>();
        CreateMap<CreateBankHolidayRequest, BankHoliday>();
    }
}

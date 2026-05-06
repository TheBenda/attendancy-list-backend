using ALB.Domain.Entities;

namespace ALB.Api.Endpoints.Groups.Mappers;

internal static class GroupsMapper
{
    extension(Group group)
    {
        
    }

    extension(AcademicYear academicYear)
    {
        internal AcademicYearDto ToDto() => 
            new(academicYear.Id, academicYear.StartDate, academicYear.EndDate);
    }

    extension(AllowedGroupname allowedGroupname)
    {
        internal AllowedGroupnamesDto ToDto() => 
            new(allowedGroupname.Id, allowedGroupname.Groupname);
    }
}
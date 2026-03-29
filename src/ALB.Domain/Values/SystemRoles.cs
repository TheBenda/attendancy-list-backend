namespace ALB.Domain.Values;

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string CoAdmin = "CoAdmin";
    public const string Team = "Team";
    public const string Parent = "Parent";

    public const string AdminPolicy = "AdminOnly";
    public const string CoAdminPolicy = "CoAdminOnly";
    public const string TeamPolicy = "TeamOnly";
    public const string ParentPolicy = "ParentOnly";
    
    public static readonly string[] Roles = [Admin, CoAdmin, Team, Parent];
}
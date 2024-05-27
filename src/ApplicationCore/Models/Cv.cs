namespace ApplicationCore.Models;

public class Cv
{
    public required string Email { get; init; }
    public List<WorkExperience> WorkExperiences { get; init; } = new();
    public List<ProjectExperience> ProjectExperiences { get; init; } = new();
    public List<Presentation> Presentations { get; init; } = new();

    public List<Certification> Certifiactions { get; init; } = new();
}

public class WorkExperience
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }

    public required string Company {get; init;}
    public DateOnly? FromDate { get; init; }
    public required DateOnly ToDate { get; init; }
}

public class ProjectExperience
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateOnly? FromDate { get; init; }
    public required DateOnly ToDate { get; init; }

    public required string Customer {get; init;}

    public List<ProjectExperienceRole> Roles { get; init; } = new();

    public HashSet<string> Competencies { get; init; } = new();
    public int? Rank {get; set; }
}

public class ProjectExperienceRole
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}

public class Presentation
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateOnly? Date { get; init; }
}

public class Certification
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }

    public required string Issuer {get; init;}
    public DateOnly? ExpiryDate { get; init; }
    public DateOnly? IssuedDate { get; init; }
}

public class Competency
{
    public required string Name { get; init; }
}

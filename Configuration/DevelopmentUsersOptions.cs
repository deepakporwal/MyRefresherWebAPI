using System.ComponentModel.DataAnnotations;

namespace MyRefresherWebAPI.Configuration;

public sealed class DevelopmentUsersOptions
{
    public const string SectionName = "DevelopmentUsers";

    [Required, MinLength(1)]
    public List<DevelopmentUserOptions> Users { get; set; } = [];
}

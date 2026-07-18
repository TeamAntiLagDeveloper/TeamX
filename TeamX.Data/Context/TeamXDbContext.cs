using Microsoft.EntityFrameworkCore;
using TeamX.Core.Entities;

namespace TeamX.Data.Context;

public class TeamXDbContext : DbContext
{

    public TeamXDbContext(DbContextOptions<TeamXDbContext> options)
        : base(options)
    {

    }


    public DbSet<License> Licenses { get; set; }

}
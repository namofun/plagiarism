using Microsoft.EntityFrameworkCore;

namespace SatelliteSite
{
    public class DevelopmentContext : DbContext
    {
        public DevelopmentContext(DbContextOptions<DevelopmentContext> options)
            : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }
}

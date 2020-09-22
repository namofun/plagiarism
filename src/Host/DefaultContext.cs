using Microsoft.EntityFrameworkCore;

namespace SatelliteSite
{
    public class DefaultContext : DbContext
    {
        public DefaultContext(DbContextOptions<DefaultContext> options)
            : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }
}

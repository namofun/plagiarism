using Microsoft.EntityFrameworkCore;

namespace SatelliteSite
{
    public class ProductionContext : DbContext
    {
        public ProductionContext(
            DbContextOptions<ProductionContext> options)
            : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }
}

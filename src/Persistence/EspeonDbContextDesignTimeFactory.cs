#if DEBUG
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Espeon {
    public class EspeonDbContextDesignTimeFactory : IDesignTimeDbContextFactory<EspeonDbContext> {
        public EspeonDbContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<EspeonDbContext>();
            optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=Espeon;Username=postgres;Password=casino");
            return new EspeonDbContext(optionsBuilder.Options);
        }
    }
}
#endif
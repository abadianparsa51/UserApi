using Microsoft.EntityFrameworkCore;

namespace UserApi.Data
{
    public class HangfireDbContext : DbContext
    {
        public HangfireDbContext(DbContextOptions<HangfireDbContext> options) : base(options) { }

    }
}

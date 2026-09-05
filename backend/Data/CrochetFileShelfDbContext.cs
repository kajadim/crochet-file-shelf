using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class CrochetFileShelfDbContext : DbContext
    {
        public CrochetFileShelfDbContext(DbContextOptions<CrochetFileShelfDbContext> options): base(options)
        { }
    }
}

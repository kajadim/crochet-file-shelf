using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class CrochetFileShelfDbContext : DbContext
    {
        public CrochetFileShelfDbContext(DbContextOptions<CrochetFileShelfDbContext> options) : base(options)
        { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<Work> Works => Set<Work>();
        public DbSet<Pattern> Patterns => Set<Pattern>();
        public DbSet<PatternCell> PatternCells => Set<PatternCell>();
        public DbSet<YarnColor> YarnColors => Set<YarnColor>();
        public DbSet<WorkComment> WorkComments => Set<WorkComment>();
        public DbSet<VideoReference> VideoReferences => Set<VideoReference>();
        public DbSet<WorkMember> WorkMembers => Set<WorkMember>();
        public DbSet<WorkInvitation> WorkInvitations => Set<WorkInvitation>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<PendingRegistration> PendingRegistrations => Set<PendingRegistration>();
        public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrochetFileShelfDbContext).Assembly);
        }
    }
}

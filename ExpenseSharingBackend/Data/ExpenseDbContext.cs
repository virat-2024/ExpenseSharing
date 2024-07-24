using ExpenseSharingBackend.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ExpenseSharingBackend.Data
{
    public class ExpenseDbContext: IdentityDbContext<ApplicationUser>
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options):base(options)
        {
            
        }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> User {  get; set; }
        public DbSet<GroupMembership> GroupMemberships { get; set; }
        public DbSet<Expanse> Expenses { get; set; }
        public DbSet<MemberPayment> MemberPayments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Expanse>(entity =>
            {
                entity.HasKey(e => e.ExpenseId);
            });
            builder.Entity<GroupMembership>()
                .HasKey(gm => new { gm.GroupId, gm.UserId });

            builder.Entity<GroupMembership>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMemberships)
                .HasForeignKey(gm => gm.GroupId);

            builder.Entity<GroupMembership>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId);
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace Step.Accounts
{
    public class AccountDbContext : AccountDbContext<Account>
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
        : base(options)
        { }

        protected AccountDbContext()
        { }
    }

    public class AccountDbContext<TAccount> : DbContext where TAccount : Account
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
        : base(options)
        {
        }

        protected  AccountDbContext() { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Account>(b =>
            {
                b.HasIndex(o => o.GlobalId);
            });

            builder.Entity<AccountToken>(b =>
            {
                b.HasKey(o => o.Hash);
                b.Property(o => o.Hash).HasMaxLength(40);
            });

            builder.Entity<AccountCode>(b =>
            {
                b.HasKey(o => o.Hash);
                b.Property(o => o.Hash).HasMaxLength(40);
            });
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountToken> AccountTokens { get; set; }
        public DbSet<AccountCode> AccountCodes { get; set; }
        public DbSet<ClientAccount> ClientAccounts { get; set; }
    }
}

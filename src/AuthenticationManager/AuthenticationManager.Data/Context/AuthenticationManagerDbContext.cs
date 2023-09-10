using AuthenticationManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationManager.Data.Context
{
    public class AuthenticationManagerDbContext<TUser> : DbContext
        where TUser : User
    {
        public DbSet<TUser> Users { get; set; }

        public DbSet<Claim> Claims { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Connection> Connections { get; set; }

        public DbSet<UserAgent> UserAgents { get; set; }

        public DbSet<UserRole> UsersRoles { get; set; }

        public DbSet<UserClaim> UsersClaims { get; set; }

        public DbSet<UserUserAgent> UsersUserAgents { get; set; }

        public AuthenticationManagerDbContext(DbContextOptions options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserRole>()
                .ToTable("AM_UsersRoles")
                .HasKey(p => new { p.UserId, p.RoleId });
            
            builder.Entity<UserClaim>()
                .ToTable("AM_UsersClaims")
                .HasKey(p => new { p.UserId, p.ClaimId });
            
            builder.Entity<UserUserAgent>()
                .ToTable("AM_UsersUserAgents")
                .HasKey(p => new { p.UserId, p.UserAgentId });

            builder.Entity<User>()
                .ToTable("AM_Users");
            
            builder.Entity<User>()
                .HasIndex(i => i.Username)
                .IsUnique();

            builder.Entity<User>()
                .HasIndex(i => i.Email)
                .IsUnique();

            builder.Entity<Role>()
                .ToTable("AM_Roles");
            
            builder.Entity<Connection>()
                .ToTable("AM_Connections");

            builder.Entity<Claim>()
                .ToTable("AM_Claims");
            
            builder.Entity<UserAgent>()
                .ToTable("AM_UserAgent");

            base.OnModelCreating(builder);
        }
    }
}
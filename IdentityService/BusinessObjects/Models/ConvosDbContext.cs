using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace BusinessObjects.Models
{
    public class ConvosDbContext : DbContext
    {

        public ConvosDbContext(DbContextOptions<ConvosDbContext> options)
            : base(options)
        { }

        // DbSet properties for all models
        public DbSet<User> Users { get; set; }


        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }


        public ConvosDbContext()
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
















        }
    }


}
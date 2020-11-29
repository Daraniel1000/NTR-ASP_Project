using Microsoft.EntityFrameworkCore;
using MySQL.Data.EntityFrameworkCore;

namespace NTR20Z
{
    public class MyContext : DbContext
    {
        public DbSet<Teacher> teachers { get; set; }
        public DbSet<Subject> subjects { get; set; }
        public DbSet<Slot> slots { get; set; }
        public DbSet<Group> groups { get; set; }
        public DbSet<Room> rooms { get; set; }
        public DbSet<Assignment> assignments { get; set; }
        public DbSet<Activity> activities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=NTR20Z;user=NTR20Z;password=12345678");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Teacher>(entity=>
            {
               entity.Property(entity=>entity.name).IsRequired().IsUnicode(false).HasMaxLength(30);
               entity.Property(entity=>entity.Comment).IsUnicode(false).HasMaxLength(4000);
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });

            modelBuilder.Entity<Subject>(entity=>
            {
               entity.Property(entity=>entity.name).IsRequired().IsUnicode(false).HasMaxLength(30);
               entity.Property(entity=>entity.Comment).IsUnicode(false).HasMaxLength(4000);
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });

            modelBuilder.Entity<Slot>(entity=>
            {
               entity.Property(entity=>entity.name).IsRequired().IsUnicode(false).HasMaxLength(30);
               entity.Property(entity=>entity.Comment).IsUnicode(false).HasMaxLength(4000);
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });

            modelBuilder.Entity<Group>(entity=>
            {
               entity.Property(entity=>entity.name).IsRequired().IsUnicode(false).HasMaxLength(30);
               entity.Property(entity=>entity.Comment).IsUnicode(false).HasMaxLength(4000);
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });

            modelBuilder.Entity<Room>(entity=>
            {
               entity.Property(entity=>entity.name).IsRequired().IsUnicode(false).HasMaxLength(30);
               entity.Property(entity=>entity.Comment).IsUnicode(false).HasMaxLength(4000);
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });

            modelBuilder.Entity<Assignment>(entity=>
            {
               //entity.HasKey(ass=>new{ass.GroupID, ass.TeacherID});
               entity.Property(entity=>entity.Comment).IsUnicode(false).HasMaxLength(4000);
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });

            modelBuilder.Entity<Activity>(entity=>
            {
               entity.Property(entity=>entity.Timestamp).HasColumnType("Timestamp").IsConcurrencyToken();
            });
        }
    }
}
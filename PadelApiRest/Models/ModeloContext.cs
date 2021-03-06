namespace PadelApiRest.Models
{
    using System.Data.Entity;

    public partial class ModeloContext : DbContext
    {
        public ModeloContext()
            : base("name=Modelo")
        {
        }

        public virtual DbSet<Reservation> Reservation { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>()
                .Property(e => e.rsvday)
                .IsUnicode(false);

            modelBuilder.Entity<Reservation>()
                .Property(e => e.rsvtime)
                .IsUnicode(false);

            modelBuilder.Entity<Reservation>()
                .Property(e => e.rsvdateTime);

            modelBuilder.Entity<User>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.phone)
                .IsUnicode(false);

            //modelBuilder.Entity<User>()
            //    .HasMany(e => e.Reservation)
            //    .WithRequired(e => e.User)
            //    .WillCascadeOnDelete(false);
        }
    }
}

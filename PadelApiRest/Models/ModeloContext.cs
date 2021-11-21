namespace PadelApiRest.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Common;

    public partial class ModeloContext : DbContext
    {
        public ModeloContext() : base(getConnectionString(), true)
        { }

        public static DbConnection getConnectionString()
        {
            string base_de_datos_elegida = System.Configuration.ConfigurationManager.AppSettings["database"];
            string modelo_cs = System.Configuration.ConfigurationManager.ConnectionStrings[base_de_datos_elegida].ConnectionString;
            modelo_cs = modelo_cs.Replace("%rutaRelativa%", AppDomain.CurrentDomain.BaseDirectory);
            if (base_de_datos_elegida == "SQLITE")
            {
                return new System.Data.SQLite.SQLiteConnection(modelo_cs);
            }
            else if (base_de_datos_elegida == "LOCALDB")
            {
                return new System.Data.SqlClient.SqlConnection(modelo_cs);
            }
            else
                throw new Exception("No se ha elegido una base de datos compatible");
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

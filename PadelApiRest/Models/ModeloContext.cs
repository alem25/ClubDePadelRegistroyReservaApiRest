namespace PadelApiRest.Models
{
    using System;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.Data.Entity;
    using System.Data.Common;
    using System.Configuration;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using MySql.Data.MySqlClient;

    public partial class ModeloContext : DbContext
    {
        public ModeloContext() : base(GetConnectionString(), true)
        { }

        public static DbConnection GetConnectionString()
        {
            string base_de_datos_elegida = ConfigurationManager.AppSettings["database"];
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jsonFilePath = string.Format("{0}credenciales.json", currentDirectory);
            JObject data = JObject.Parse(File.ReadAllText(jsonFilePath));
            string cs = data[base_de_datos_elegida].ToString();
            cs = cs.Replace("%rutaRelativa%", AppDomain.CurrentDomain.BaseDirectory);
            if (base_de_datos_elegida == "SQLITE")
            {
                var conn = new SQLiteConnection(cs);
                try
                {
                    conn.Open();
                    return conn;
                }
                catch (Exception ex) 
                {
                    CredencialesIncorrectas(ex.Message);
                }
            }
            else if (base_de_datos_elegida == "LOCALDB" || base_de_datos_elegida == "SQLSERVER")
            {
                var conn = new SqlConnection(cs);
                try
                {
                    conn.Open();
                    return conn;
                }
                catch (Exception ex) 
                {
                    CredencialesIncorrectas(ex.Message);
                }
            }
            else if (base_de_datos_elegida == "MYSQL" || base_de_datos_elegida == "MARIADB")
            {
                var conn = new MySqlConnection(cs);
                try
                {
                    conn.Open();
                    return conn;
                }
                catch (Exception ex)
                {
                    CredencialesIncorrectas(ex.Message);
                }
            }
            throw new Exception("No se ha elegido una base de datos compatible");
        }

        private static Exception CredencialesIncorrectas(string msg)
        {
            string mensaje = "Las credenciales de la base de datos elegida no son válidas. ";
            if (!string.IsNullOrEmpty(msg))
                mensaje = string.Format("{0} Error: {1}", mensaje, msg);
            throw new Exception(mensaje);
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

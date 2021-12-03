namespace PadelApiRest.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("User"), JsonObject( IsReference = true)]
    public partial class User
    {
        public User()
        {
            Reservation = new HashSet<Reservation>();
        }

        [Key]
        [StringLength(50)]
        public string username { get; set; }

        [Required]
        [StringLength(256)]
        public string password { get; set; }

        [JsonIgnore]
        public int salt { get; set; }

        [Required]
        [StringLength(50)]
        public string email { get; set; }

        [StringLength(12)]
        public string phone { get; set; }

        public long? birthdate { get; set; }

        public virtual ICollection<Reservation> Reservation { get; set; }
    }
}

namespace PadelApiRest.Models
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Reservation"), JsonObject(IsReference = true)]
    public partial class Reservation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public int rsvId { get; set; }

        [Required]
        public int courtId { get; set; }

        [Required]
        public long rsvdateTime { get; set; }

        [Required]
        [StringLength(50)]
        public string rsvday { get; set; }

        [Required]
        [StringLength(50)]
        public string rsvtime { get; set; }

        public string username { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

        public Reservation() { }
        public Reservation(int courtId, long ticks)
        {
            this.courtId = courtId;
            this.rsvday = DateTimeOffset.FromUnixTimeMilliseconds(ticks).LocalDateTime.ToString("yyyy/MM/dd");
            this.rsvtime = DateTimeOffset.FromUnixTimeMilliseconds(ticks).LocalDateTime.ToString("HH:mm");
            this.rsvdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PadelApiRest.Models
{
    public class Reservation
    {
        public int rsvId { get; set; }
        [Required]
        [Range(1, 4, ErrorMessage = "Este campo sólo admite enteros del 1 al 4.")]
        public int courtId { get; set; }
        [Required]
        public long rsvdateTime { get; set; }
        public string rsvday { get; set; }
        public string rsvtime { get; set; }

        public Reservation()
        {

        }

        public Reservation(int courtId, long ticks)
        {
            this.courtId = courtId;
            this.rsvday = DateTimeOffset.FromUnixTimeMilliseconds(ticks).LocalDateTime.ToString("yyyy/MM/dd");
            this.rsvtime = DateTimeOffset.FromUnixTimeMilliseconds(ticks).LocalDateTime.ToString("HH:mm");
            this.rsvdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}
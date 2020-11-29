namespace PadelApiRest.Models
{
    public class Reservation
    {
        public int rsvId { get; set; }
        public int courtId { get; set; }
        public int rsvdateTime { get; set; }
        public string rsvday { get; set; }
        public string rsvtime { get; set; }

        public Reservation()
        {

        }

        public Reservation(int rsvId, int courtId, int rsvdateTime, string rsvday, string rsvtime)
        {
            this.rsvId = rsvId;
            this.courtId = courtId;
            this.rsvdateTime = rsvdateTime;
            this.rsvday = rsvday;
            this.rsvtime = rsvtime;
        }
    }
}
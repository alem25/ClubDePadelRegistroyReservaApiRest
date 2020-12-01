using System;
using System.Collections.Generic;

namespace PadelApiRest.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

        public User()
        {

        }

        public User(string username, string email, string password, string phone = null, DateTime? birthDate = null)
        {
            this.Username = username;
            this.Email = email;
            this.Password = password;
            this.Phone = phone;
            this.BirthDate = birthDate;
        }
    }
}
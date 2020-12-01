using PadelApiRest.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PadelApiRest.Controllers
{
    public class ReservationsController : ApiController
    {
        // GET api/reservations
        public IEnumerable<Reservation> Get()
        {
            string username = HomeController.GetAuthorizationHeaderUsername(Request);
            HomeController.ValidateAuthorizationHeader(Request, username);
            HomeController.CreateOrUpdateAuthorizationHeader(Request, username);
            List<Reservation> reservations = new List<Reservation>();
            using (SqlConnection con = HomeController.ConnectToSql())
            {
                string query = "SELECT * FROM " + nameof(Reservation) + "WHERE " + nameof(Reservation.rsvId) +
                    " IN (SELECT " + nameof(Reservation.rsvId) + " FROM UserReservations WHERE " + nameof(Models.User.Username) + " = " + username + ")";
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        reservations.Add(new Reservation()
                        {
                            rsvId = Convert.ToInt32(reader[nameof(Reservation.rsvId)]),
                            courtId = Convert.ToInt32(reader[nameof(Reservation.courtId)]),
                            rsvdateTime = Convert.ToInt64(reader[nameof(Reservation.rsvdateTime)]),
                            rsvday = reader[nameof(Reservation.rsvday)].ToString(),
                            rsvtime = reader[nameof(Reservation.rsvtime)].ToString()
                        });
                    }
                    return reservations;
                }
            }
        }

        // GET api/reservations/5
        public Reservation Get(long unixTime)
        {
            string username = HomeController.GetAuthorizationHeaderUsername(Request);
            HomeController.ValidateAuthorizationHeader(Request, username);
            HomeController.CreateOrUpdateAuthorizationHeader(Request, username);
            string day = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).LocalDateTime.ToString("yyyy/MM/dd");
            using (SqlConnection con = HomeController.ConnectToSql())
            {
                string query = "SELECT * FROM " + nameof(Reservation) + " WHERE " + nameof(Reservation.rsvday) + " = " + day;
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        return new Reservation()
                        {
                            rsvId = Convert.ToInt32(reader[nameof(Reservation.rsvId)]),
                            courtId = Convert.ToInt32(reader[nameof(Reservation.courtId)]),
                            rsvdateTime = Convert.ToInt64(reader[nameof(Reservation.rsvdateTime)]),
                            rsvday = reader[nameof(Reservation.rsvday)].ToString(),
                            rsvtime = reader[nameof(Reservation.rsvtime)].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // POST api/reservations
        public void Post([FromBody]Reservation reservation)
        {
            string username = HomeController.GetAuthorizationHeaderUsername(Request);
            HomeController.ValidateAuthorizationHeader(Request, username);
            HomeController.CreateOrUpdateAuthorizationHeader(Request, username);
            if (reservation != null && reservation.rsvdateTime != 0 && reservation.courtId != 0)
            {
                Reservation newReservation = new Reservation(reservation.courtId, reservation.rsvdateTime);
                using (SqlConnection con = HomeController.ConnectToSql())
                {
                    string query = "INSERT INTO " + nameof(Reservation) + " VALUES(" + newReservation.courtId + ", " +
                        newReservation.rsvdateTime + ", '" + newReservation.rsvday + "', '" + newReservation.rsvtime + "')";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        con.Open();
                        int filasAfectadas = command.ExecuteNonQuery();
                        Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                    }
                }
            }
        }

        // DELETE api/reservations/5
        public void Delete(int id)
        {
            string username = HomeController.GetAuthorizationHeaderUsername(Request);
            HomeController.ValidateAuthorizationHeader(Request, username);
            HomeController.CreateOrUpdateAuthorizationHeader(Request, username);
            using (SqlConnection con = HomeController.ConnectToSql())
            {
                string query = "SELECT * FROM " + nameof(Reservation) + " WHERE " + nameof(Reservation.rsvId) + " = " + id;
                using (SqlCommand command = new SqlCommand(query))
                {
                    con.Open();
                    int filasAfectadas = command.ExecuteNonQuery();
                    Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                }
            }
        }

        // DELETE api/reservations
        public void Delete()
        {
            string username = HomeController.GetAuthorizationHeaderUsername(Request);
            HomeController.ValidateAuthorizationHeader(Request, username);
            HomeController.CreateOrUpdateAuthorizationHeader(Request, username);
            using (SqlConnection con = HomeController.ConnectToSql())
            {
                string query = "SELECT * FROM " + nameof(Reservation);
                using (SqlCommand command = new SqlCommand(query))
                {
                    con.Open();
                    int filasAfectadas = command.ExecuteNonQuery();
                    Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                }
            }
        }
    }
}

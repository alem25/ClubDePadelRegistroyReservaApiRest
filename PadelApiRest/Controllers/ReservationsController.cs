using PadelApiRest.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PadelApiRest.Controllers
{
    public class ReservationsController : ApiController
    {
        public SqlConnection ConnectToSql()
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            return connection;
        }
        // GET api/reservations
        public IEnumerable<Reservation> Get()
        {
            return null;
        }

        // GET api/reservations/5
        public Reservation Get(int id)
        {
            using (SqlConnection con = ConnectToSql())
            {
                string query = "SELECT * FROM " + nameof(Reservation) + " WHERE " + nameof(Reservation.rsvId) + " = " + id;
                using (SqlCommand command = new SqlCommand(query))
                {
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        return new Reservation()
                        {
                            rsvId = Convert.ToInt32(reader[nameof(Reservation.rsvId)]),
                            courtId = Convert.ToInt32(reader[nameof(Reservation.courtId)]),
                            rsvdateTime = Convert.ToInt32(reader[nameof(Reservation.rsvdateTime)]),
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
        }

        // PUT api/reservations/5
        public void Put(int id, [FromBody]Reservation reservation)
        {
        }

        // DELETE api/reservations/5
        public void Delete(int id)
        {
        }
    }
}

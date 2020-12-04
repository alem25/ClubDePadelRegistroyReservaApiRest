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
        private const string ERROR = "Error del servidor.";

        // GET api/reservations
        public IEnumerable<Reservation> Get()
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                List<Reservation> reservations = new List<Reservation>();
                using (SqlConnection con = HomeController.ConnectToSql())
                {
                    string query = "SELECT * FROM " + nameof(Reservation) + " WHERE username = '" + username + "'";
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
            catch(Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        // GET api/reservations/5
        public IEnumerable<Reservation> Get(long id)
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                var result = new List<Reservation>();
                string day = DateTimeOffset.FromUnixTimeMilliseconds(id).LocalDateTime.ToString("yyyy/MM/dd");
                using (SqlConnection con = HomeController.ConnectToSql())
                {
                    string query = "SELECT * FROM " + nameof(Reservation) + " WHERE " + nameof(Reservation.rsvday) + " = '" + day + "'";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            result.Add(new Reservation()
                            {
                                rsvId = Convert.ToInt32(reader[nameof(Reservation.rsvId)]),
                                courtId = Convert.ToInt32(reader[nameof(Reservation.courtId)]),
                                rsvdateTime = Convert.ToInt64(reader[nameof(Reservation.rsvdateTime)]),
                                rsvday = reader[nameof(Reservation.rsvday)].ToString(),
                                rsvtime = reader[nameof(Reservation.rsvtime)].ToString()
                            });
                        }
                    }
                }
                return result;
            }
            catch(Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        // POST api/reservations
        public void Post([FromBody]Reservation reservation)
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                if (reservation != null && reservation.rsvdateTime != 0 && reservation.courtId > 0 && reservation.courtId < 5)
                {
                    Reservation newReservation = new Reservation(reservation.courtId, reservation.rsvdateTime);
                    List<Reservation> res = Get(reservation.rsvdateTime).ToList();
                    if(res.Any(r => r.courtId == newReservation.courtId && r.rsvday == newReservation.rsvday && r.rsvtime == newReservation.rsvtime))
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.Conflict, "Esta pista está reservada para la fecha y hora indicada.");
                    using (SqlConnection con = HomeController.ConnectToSql())
                    {
                        int reservaId = 0;
                        string query = "INSERT INTO " + nameof(Reservation) + " OUTPUT INSERTED."+ nameof(Reservation.rsvId) +" VALUES(" + newReservation.courtId + ", " +
                            newReservation.rsvdateTime + ", '" + newReservation.rsvday + "', '" + newReservation.rsvtime + "', '" + username + "')";
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            con.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                reservaId = Convert.ToInt32(reader[nameof(Reservation.rsvId)]);
                            }
                            con.Close();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        [AcceptVerbs("DELETE")]
        public HttpResponseMessage Delete(int id)
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                using (SqlConnection con = HomeController.ConnectToSql())
                {
                    string query = "DELETE " + nameof(Reservation) + " WHERE " + nameof(Reservation.rsvId) + " = " + id + " AND username = '" + username + "'";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        con.Open();
                        int filasAfectadas = command.ExecuteNonQuery();
                        Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                    }
                }
            }
            catch(Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("DELETE")]
        public HttpResponseMessage Delete()
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                using (SqlConnection con = HomeController.ConnectToSql())
                {
                    string query = "DELETE " + nameof(Reservation) + " WHERE username = '" + username + "'";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        con.Open();
                        int filasAfectadas = command.ExecuteNonQuery();
                        Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                    }
                }
            }
            catch(Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}

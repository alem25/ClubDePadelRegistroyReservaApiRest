using Newtonsoft.Json;
using PadelApiRest.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace PadelApiRest.Controllers
{
    [RoutePrefix("api/reservations")]
    public class ReservationsController : ApiController
    {
        private const string ERROR = "Error del servidor.";
        private ModeloContext db = new ModeloContext();
        private string[] horas = new string[]
        {
            "10:00",
            "11:00",
            "12:00",
            "13:00",
            "14:00",
            "15:00",
            "16:00",
            "17:00",
            "18:00",
            "19:00",
            "20:00",
            "21:00"
        };
        private int[] courts = new int[]
        {
            1,
            2,
            3,
            4
        };

        // GET api/reservations
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                var userReservations = db.Reservation.Where(r => r.username == username);
                response.Content = new StringContent(JsonConvert.SerializeObject(userReservations), Encoding.UTF8, "application/json");
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        [AcceptVerbs("GET"), Route("{id}")]
        public HttpResponseMessage Get(long id)
        {
            HttpResponseMessage response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                string day = DateTimeOffset.FromUnixTimeMilliseconds(id).LocalDateTime.ToString("yyyy/MM/dd");
                var reservationsByDay = db.Reservation.Where(r => r.rsvday == day).ToList();
                response.Content = new StringContent(JsonConvert.SerializeObject(reservationsByDay), Encoding.UTF8, "application/json");
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        // POST api/reservations
        public HttpResponseMessage Post([FromBody]Reservation reservation)
        {
            HttpResponseMessage response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            if (reservation != null && reservation.rsvdateTime > DateTimeOffset.Now.ToUnixTimeMilliseconds() && courts.Contains(reservation.courtId))
            {
                var user = db.User.FirstOrDefault(u => u.username == username);
                Reservation newReservation = new Reservation(reservation.courtId, reservation.rsvdateTime);
                newReservation.username = username;
                if (horas.Contains(newReservation.rsvtime))
                {
                    string day = DateTimeOffset.FromUnixTimeMilliseconds(reservation.rsvdateTime).LocalDateTime.ToString("yyyy/MM/dd");
                    var reservationsByDay = db.Reservation.Where(r => r.rsvday == day).ToList();
                    if (reservationsByDay.Any(r => r.username == username && r.rsvday == newReservation.rsvday && r.rsvtime == newReservation.rsvtime))
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Ya tienes otra reserva para el mismo día y hora indicada.");
                    if (reservationsByDay.Any(r => r.courtId == newReservation.courtId && r.rsvday == newReservation.rsvday && r.rsvtime == newReservation.rsvtime))
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Esta pista está reservada para la fecha y hora indicada.");
                    var userReservations = db.Reservation.Where(r => r.username == username).ToList();
                    if (userReservations.Count > 3)
                        return Request.CreateResponse(HttpStatusCode.Conflict, "Solo puedes realizar un máximo de 4 reservas.");
                    try
                    {
                        db.Reservation.Add(newReservation);
                        db.SaveChanges();
                        response.StatusCode = HttpStatusCode.OK;
                        return response;
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [AcceptVerbs("DELETE"), Route("{id}")]
        public HttpResponseMessage Delete(long id)
        {
            HttpResponseMessage response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                var reserva = db.Reservation.Where(r => r.rsvId == id && r.User.username == username).FirstOrDefault();
                if (reserva == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "reserva no encontrada");
                db.Reservation.Remove(reserva);
                db.SaveChanges();
                response.StatusCode = HttpStatusCode.NoContent;
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        [AcceptVerbs("DELETE")]
        public HttpResponseMessage Delete()
        {
            HttpResponseMessage response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                var reservas = db.Reservation.Where(r => r.User.username == username).ToList();
                if (reservas.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "no hay reservas a tu nombre");
                db.Reservation.RemoveRange(reservas);
                db.SaveChanges();
                response.StatusCode = HttpStatusCode.NoContent;
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }
    }
}
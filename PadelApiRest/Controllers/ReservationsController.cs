using PadelApiRest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PadelApiRest.Controllers
{
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
        public IEnumerable<Reservation> Get()
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                return db.Reservation.Where(r => r.username == username);
            }
            catch (Exception e)
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
                string day = DateTimeOffset.FromUnixTimeMilliseconds(id).LocalDateTime.ToString("yyyy/MM/dd");
                return db.Reservation.Where(r => r.rsvday == day).ToList();
            }
            catch (Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }

        // POST api/reservations
        public void Post([FromBody]Reservation reservation)
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            if (reservation != null && reservation.rsvdateTime > DateTimeOffset.Now.ToUnixTimeMilliseconds() && courts.Contains(reservation.courtId))
            {
                var user = db.User.FirstOrDefault(u => u.username == username);
                Reservation newReservation = new Reservation(reservation.courtId, reservation.rsvdateTime);
                newReservation.username = username;
                if (horas.Contains(newReservation.rsvtime))
                {
                    List<Reservation> reservationsByDay = Get(reservation.rsvdateTime).ToList();
                    if (reservationsByDay.Any(r => r.username == username && r.rsvday == newReservation.rsvday && r.rsvtime == newReservation.rsvtime))
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.BadRequest, "Ya tienes otra reserva para el mismo día y hora indicada.");
                    if (reservationsByDay.Any(r => r.courtId == newReservation.courtId && r.rsvday == newReservation.rsvday && r.rsvtime == newReservation.rsvtime))
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.BadRequest, "Esta pista está reservada para la fecha y hora indicada.");
                    List<Reservation> userReservations = Get().ToList();
                    if (userReservations.Count > 3)
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.Conflict, "Solo puedes realizar un máximo de 4 reservas.");
                    try
                    {
                        db.Reservation.Add(newReservation);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                    }
                }
            }
        }

        [AcceptVerbs("DELETE")]
        public HttpResponseMessage Delete(int id)
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                var reserva = db.Reservation.Where(r => r.rsvId == id && r.User.username == username).FirstOrDefault();
                if (reserva != null)
                {
                    db.Reservation.Remove(reserva);
                    db.SaveChanges();
                }
                else
                    throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.NotFound, "reserva no encontrada");
            }
            catch (Exception e)
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
                var reservas = db.Reservation.Where(r => r.User.username == username).ToList();
                if (reservas.Count > 0)
                {
                    db.Reservation.RemoveRange(reservas);
                    db.SaveChanges();
                }
                else
                    throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.NotFound, "no hay reservas a tu nombre");
            }
            catch (Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}

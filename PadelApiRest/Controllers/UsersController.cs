using PadelApiRest.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Text;

namespace PadelApiRest.Controllers
{
    public class UsersController : ApiController
    {
        private const string ERROR = "Error del servidor.";
        private ModeloContext db = new ModeloContext();

        // POST: api/users
        public HttpResponseMessage Post([FromBody]User user)
        {
            if (user != null && !string.IsNullOrEmpty(user.username) && !string.IsNullOrEmpty(user.password) && !string.IsNullOrWhiteSpace(user.email))
            {
                var usuarioDb = db.User.FirstOrDefault(u => u.username == user.username);
                if (usuarioDb == null)
                {
                    try
                    {
                        user.salt = HomeController.CreateSalt();
                        user.password = HomeController.HashPassword(user.password, user.salt);
                        db.User.Add(user);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.Created);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                    }
                }
                return Request.CreateResponse(HttpStatusCode.Conflict, "usuario duplicado");
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "usuario no válido");
        }

        [Route("api/users/Login"), AcceptVerbs("GET")]
        public HttpResponseMessage Login(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                User user = db.User.FirstOrDefault(u => u.username == username);
                if (user != null)
                {
                    if (HomeController.ComparePasswords(user.password, user.salt, password))
                        return HomeController.CreateAuthorizationHeader(Request, username);
                    else
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "usuario o contraseña incorrecta");
                }
                return Request.CreateResponse(HttpStatusCode.NotFound, "el usuario no existe");
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "usuario o contraseña vacía");
        }

        public HttpResponseMessage Get(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    User user = db.User.FirstOrDefault(u => u.username == id);
                    if (user != null)
                    {
                        return Request.CreateResponse(user.username);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                }
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "usuario no encontrado");
        }

        [AcceptVerbs("DELETE")]
        public HttpResponseMessage Delete(string id)
        {
            HttpResponseMessage response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                if (id == username)
                {
                    var user = db.User.FirstOrDefault(u => u.username == username);
                    if (user == null)
                        return Request.CreateResponse(HttpStatusCode.NotFound, "usuario no encontrado");
                    db.User.Remove(user);
                    db.SaveChanges();
                    response.StatusCode = HttpStatusCode.NoContent;
                    return response;
                }
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "usuario no autorizado");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
        }
    }
}

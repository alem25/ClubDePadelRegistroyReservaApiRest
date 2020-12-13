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
                        Random r = new Random(DateTimeOffset.Now.Millisecond);
                        var salt = r.Next(1000,9999);
                        byte[] bytes = Encoding.UTF8.GetBytes(user.password + salt);
                        var crypto = System.Security.Cryptography.SHA256.Create();
                        byte[] hash = crypto.ComputeHash(bytes);
                        user.password = HomeController.ByteArrayToString(hash);
                        user.salt = salt;
                        db.User.Add(user);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                    }
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                    throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.Conflict, "usuario duplicado");
            }
            else
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.BadRequest, "usuario no válido");
        }

        [Route("api/users/Login"), AcceptVerbs("GET")]
        public HttpResponseMessage Login(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                User user = db.User.FirstOrDefault(u => u.username == username);
                if (user != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(password + user.salt);
                    var crypto = System.Security.Cryptography.SHA256.Create();
                    byte[] hash = crypto.ComputeHash(bytes);
                    password = HomeController.ByteArrayToString(hash);
                    if (user.password != password)
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.Unauthorized, "usuario o contraseña incorrecta");
                    else
                        return HomeController.CreateAuthorizationHeader(Request, username);
                }
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.NotFound, "el usuario no existe");
            }
            throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.BadRequest, "usuario o contraseña vacía");
        }

        public string Get(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    User user = db.User.FirstOrDefault(u => u.username == id);
                    if (user != null)
                    {
                        return user.username;
                    }
                }
                catch (Exception e)
                {
                    throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                }
            }
            throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.NotFound, "usuario no encontrado");
        }

        [AcceptVerbs("DELETE")]
        public HttpResponseMessage Delete(string id)
        {
            HttpResponseMessage Response = HomeController.ValidateAuthorizationHeader(Request, out string username);
            try
            {
                if (id == username)
                {
                    var user = db.User.FirstOrDefault(u => u.username == username);
                    if(user != null)
                    {
                        db.User.Remove(user);
                        db.SaveChanges();
                    }
                    else
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.NotFound, "usuario no encontrado");
                }
                else
                    throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.Unauthorized, "usuario no autorizado");
            }
            catch (Exception e)
            {
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
            }
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}

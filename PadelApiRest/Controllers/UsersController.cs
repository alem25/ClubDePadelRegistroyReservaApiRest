using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PadelApiRest.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Diagnostics;

namespace PadelApiRest.Controllers
{
    public class UsersController : ApiController
    {
        private const string ERROR = "Error del servidor.";

        // POST: api/users
        public HttpResponseMessage Post([FromBody]User user)
        {
            if (user != null && !string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(user.Password) && !string.IsNullOrWhiteSpace(user.Email))
            {
                HttpStatusCode result = HttpStatusCode.OK;
                try
                {
                    string userr = Get(user.Username);
                }
                catch(HttpResponseException e)
                {
                    result = e.Response.StatusCode;
                }
                
                if (result == HttpStatusCode.NotFound)
                {
                    try
                    {
                        User newUser = new User(user.Username, user.Email, user.Password, user.Phone, user.BirthDate);
                        using (SqlConnection con = HomeController.ConnectToSql())
                        {
                            string query = "INSERT INTO [" + nameof(Models.User) + "] VALUES('" + newUser.Username + "', '" +
                                newUser.Password + "', '" + newUser.Email + "', '" + newUser.Phone + "', '" + newUser.BirthDate + "')";
                            using (SqlCommand command = new SqlCommand(query, con))
                            {
                                con.Open();
                                int filasAfectadas = command.ExecuteNonQuery();
                                Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                            }
                        }
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
                string content = Get(username);
                if (content != null && content.Trim() != string.Empty && content.Trim() == username.Trim())
                {
                    string pass = string.Empty;
                    try
                    {
                        using (SqlConnection con = HomeController.ConnectToSql())
                        {
                            string query = "SELECT " + nameof(Models.User.Password) + " FROM [" + nameof(Models.User) + "] WHERE " + nameof(Models.User.Username) + " = '" + username.ToUpper().Trim() + "'";
                            using (SqlCommand command = new SqlCommand(query, con))
                            {
                                con.Open();
                                SqlDataReader reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    pass = reader[nameof(Models.User.Password)].ToString();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.InternalServerError, string.Format("{0} - {1}", ERROR, e.Message));
                    }
                    if (pass != password)
                        throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.Unauthorized, "usuario o contraseña incorrecta");
                    else
                    {
                        HttpResponseMessage response = HomeController.CreateAuthorizationHeader(Request, username);
                        return response;
                    }
                }
                throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.NotFound, "el usuario no existe");
            }
            throw HomeController.CreateResponseExceptionWithMsg(Request, HttpStatusCode.BadRequest, "usuario o contraseña vacía");
        }

        public string Get(string id)
        {
            string user = string.Empty;
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    using (SqlConnection con = HomeController.ConnectToSql())
                    {
                        string query = "SELECT " + nameof(Models.User.Username) + " FROM [" + nameof(Models.User) + "] WHERE " + nameof(Models.User.Username) + " = '" + id.ToUpper().Trim() + "'";
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            con.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                user = reader[nameof(Models.User.Username)].ToString();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(user))
                    {
                        if (id.Trim().ToUpper() == user.Trim().ToUpper())
                            return user;
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
                    using (SqlConnection con = HomeController.ConnectToSql())
                    {
                        string query = "DELETE [" + nameof(Models.User) + "] WHERE " + nameof(Models.User.Username) + " = '" + username + "'";
                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            con.Open();
                            int filasAfectadas = command.ExecuteNonQuery();
                            Debug.WriteLine("Número de filas afectadas: " + filasAfectadas);
                        }
                    }
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

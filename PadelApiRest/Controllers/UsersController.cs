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
        // POST: api/users
        public void Post([FromBody]User user)
        {
            if(user != null && !string.IsNullOrEmpty(user.Username) && !string.IsNullOrEmpty(user.Password) && !string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.Phone))
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
        }

        public void Get(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && Get(username))
            {
                string pass = string.Empty;
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
                        if (pass != password)
                            throw new HttpResponseException(HttpStatusCode.Unauthorized);
                        else
                        {
                            Request = HomeController.CreateOrUpdateAuthorizationHeader(Request, username);
                        }
                    }
                }
            }
            else
                throw new HttpResponseException(HttpStatusCode.BadRequest);
        }

        public bool Get(string username)
        {
            string user = string.Empty;
            if (!string.IsNullOrEmpty(username))
            {
                using (SqlConnection con = HomeController.ConnectToSql())
                {
                    string query = "SELECT " + nameof(Models.User.Username) + " FROM [" + nameof(Models.User) + "] WHERE " + nameof(Models.User.Username) + " = '" + username.ToUpper().Trim() + "'";
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
                if (!String.IsNullOrEmpty(user))
                    return username.Trim().ToUpper() == user.Trim().ToUpper();
            }
            return false;
        }
    }
}

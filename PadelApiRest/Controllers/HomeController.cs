using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;

namespace PadelApiRest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public static SqlConnection ConnectToSql()
        {
            SqlConnection connection = new SqlConnection
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString
            };
            return connection;
        }

        public static string GetAuthorizationHeaderUsername(HttpRequestMessage Request)
        {
            var authorization = Request.Headers.GetValues("Authorization").First();
            if (!string.IsNullOrEmpty(authorization))
            {
                string base64 = authorization.Split('.')[1];
                int mod4 = base64.Length % 4;
                if (mod4 > 0)
                    base64 += new string('=', 4 - mod4);
                byte[] decodedValue = Convert.FromBase64String(base64);
                string decodedString = Encoding.UTF8.GetString(decodedValue);
                JObject obj = JsonConvert.DeserializeObject<JObject>(decodedString);
                if (obj["user"] != null && obj["user"].ToString().Trim() != string.Empty)
                    return obj["user"].ToString().Trim();
            }
            return null;
        }

        public static void ValidateAuthorizationHeader(HttpRequestMessage Request, string username)
        {
            try
            {
                if(username == null)
                    throw new HttpResponseException(HttpStatusCode.Unauthorized);
                var authorization = Request.Headers.GetValues("Authorization").First();
                if (!string.IsNullOrEmpty(authorization))
                {
                    string base64 = authorization.Split('.')[1];
                    int mod4 = base64.Length % 4;
                    if (mod4 > 0)
                        base64 += new string('=', 4 - mod4);
                    byte[] decodedValue = Convert.FromBase64String(base64);
                    string decodedString = Encoding.UTF8.GetString(decodedValue);
                    JObject obj = JsonConvert.DeserializeObject<JObject>(decodedString);
                    if (obj["user"] == null || obj["user"].ToString().Trim() != username.Trim() || obj["exp"] == null || DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(obj["exp"])).DateTime < DateTime.Now)
                        throw new HttpResponseException(HttpStatusCode.Unauthorized);
                }
            } catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        public static HttpRequestMessage CreateOrUpdateAuthorizationHeader(HttpRequestMessage Request, string username)
        {
            Request.Headers.TryGetValues("Authorization", out IEnumerable<string> authorization);
            if (authorization != null && !string.IsNullOrEmpty(authorization.FirstOrDefault()))
                Request.Headers.Remove("Authorization");
            var claims = new Dictionary<string, object>
            {
                { "user", username }
            };
            DateTime today = DateTime.Now;
            string securityKey = ConfigurationManager.AppSettings.Get("securityKey");
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(securityKey));
            JwtSecurityTokenHandler jwth = new JwtSecurityTokenHandler();
            JwtSecurityToken token = jwth.CreateJwtSecurityToken(
                issuer: "PadelApiRest",
                audience: "PadelApiRest",
                subject: null,
                notBefore: today,
                expires: today.AddMinutes(15),
                issuedAt: today,
                signingCredentials: new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature),
                encryptingCredentials: null,
                claimCollection: claims
                );
            Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwth.WriteToken(token));
            return Request;
        }
    }
}

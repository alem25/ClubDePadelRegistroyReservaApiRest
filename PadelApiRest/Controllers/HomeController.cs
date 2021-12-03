using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;

namespace PadelApiRest.Controllers
{
    public class HomeController : Controller
    {
        private const string NO_AUTORIZADO = "Usuario no autorizado.";
        private const string ERROR = "Error del servidor.";
        private static readonly string MY_SECRET = ConfigurationManager.AppSettings.Get("securityKey");

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        public static HttpResponseMessage ValidateAuthorizationHeader(HttpRequestMessage request, out string username)
        {
            username = string.Empty;
            if (request.Headers.TryGetValues("Authorization", out IEnumerable<string> values))
            {
                try
                {
                    string authorization = values.First().Split(' ')[1];
                    SymmetricSecurityKey mySecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(MY_SECRET));
                    JwtSecurityTokenHandler jwth = new JwtSecurityTokenHandler();
                    var claims = jwth.ValidateToken(authorization, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        IssuerSigningKey = mySecretKey
                    }, out SecurityToken validatedToken);
                    if (claims.FindFirst("user") != null && claims.FindFirst("user").Value.Trim() != string.Empty)
                    {
                        username = claims.FindFirst("user").Value.Trim();
                        return CreateAuthorizationHeader(request, username);
                    }
                }
                catch
                {
                    return request.CreateResponse(HttpStatusCode.Unauthorized, NO_AUTORIZADO);
                }
            }
            return request.CreateResponse(HttpStatusCode.Unauthorized, NO_AUTORIZADO);
        }

        public static HttpResponseMessage CreateAuthorizationHeader(HttpRequestMessage request, string username)
        {
            try
            {
                if (request.Headers.TryGetValues("Authorization", out IEnumerable<string> values))
                    request.Headers.Remove("Authorization");
                string bearer = GenerateNewToken(username);
                var response = request.CreateResponse();
                response.Headers.Add("Authorization", bearer);
                return response;
            }
            catch
            {
                return request.CreateResponse(HttpStatusCode.InternalServerError, ERROR);
            }
        }

        public static string GenerateNewToken(string username)
        {
            SymmetricSecurityKey mySecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(MY_SECRET));
            JwtSecurityTokenHandler jwth = new JwtSecurityTokenHandler();
            var newClaims = new Dictionary<string, object> { { "user", username } };
            DateTime today = DateTime.Now;
            JwtSecurityToken token = jwth.CreateJwtSecurityToken(
                issuer: "PadelApiRest",
                audience: "PadelApiRest",
                subject: null,
                notBefore: today,
                expires: today.AddMinutes(10),
                issuedAt: today,
                signingCredentials: new SigningCredentials(mySecretKey, SecurityAlgorithms.HmacSha256Signature),
                encryptingCredentials: null,
                claimCollection: newClaims
                );
            return "Bearer " + jwth.WriteToken(token);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static bool ComparePasswords(string hash, int? salt, string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
            var crypto = System.Security.Cryptography.SHA256.Create();
            byte[] arrayBytesPassword = crypto.ComputeHash(bytes);
            string hashedPassword = ByteArrayToString(arrayBytesPassword);
            return hashedPassword == hash;
        }

        public static int CreateSalt()
        {
            Random r = new Random(DateTimeOffset.Now.Millisecond);
            return r.Next(1000, 9999);
        }

        public static string HashPassword(string password, int? salt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
            var crypto = System.Security.Cryptography.SHA256.Create();
            byte[] hash = crypto.ComputeHash(bytes);
            return ByteArrayToString(hash);
        }
    }
}

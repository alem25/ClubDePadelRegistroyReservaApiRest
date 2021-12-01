using Microsoft.IdentityModel.Tokens;
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
        private const string NO_AUTORIZADO = "Usuario no autorizado.";
        private const string ERROR = "Error del servidor.";
        private static string MY_SECRET = ConfigurationManager.AppSettings.Get("securityKey");

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
                    throw CreateResponseExceptionWithMsg(request, HttpStatusCode.Unauthorized, NO_AUTORIZADO);
                }
            }
            throw CreateResponseExceptionWithMsg(request, HttpStatusCode.Unauthorized, NO_AUTORIZADO);
        }

        public static HttpResponseMessage CreateAuthorizationHeader(HttpRequestMessage request, string username)
        {
            try
            {
                if (request.Headers.TryGetValues("Authorization", out IEnumerable<string> values))
                    request.Headers.Remove("Authorization");

                SymmetricSecurityKey mySecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(MY_SECRET));
                JwtSecurityTokenHandler jwth = new JwtSecurityTokenHandler();
                HttpResponseMessage response = request.CreateResponse();
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
                response.Headers.Add("Authorization", "Bearer " + jwth.WriteToken(token));
                return response;
            }
            catch
            {
                throw CreateResponseExceptionWithMsg(request, HttpStatusCode.InternalServerError, ERROR);
            }
        }

        public static HttpResponseException CreateResponseExceptionWithMsg(HttpRequestMessage Request, HttpStatusCode statusCode, string message)
        {
            HttpResponseMessage errorResponse = Request.CreateResponse(statusCode, message);
            return new HttpResponseException(errorResponse);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}

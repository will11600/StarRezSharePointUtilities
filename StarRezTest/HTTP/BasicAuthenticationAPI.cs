using StarRezTest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.HTTP
{
    public abstract class BasicAuthenticationAPI
    {
        protected static readonly string Username;
        protected static readonly string Password;

        static BasicAuthenticationAPI()
        {
            Credentials credentials = Secrets.GetCredentials("organisation");

            Username = credentials.Username;
            Password = credentials.Password;
        }

        protected static string GetAuthString()
        {
            byte[] bytes = Encoding.ASCII.GetBytes($"{Username}:{Password}");
            return Convert.ToBase64String(bytes);
        }
    }
}

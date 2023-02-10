using FTPUpdaterAPI.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;

namespace FTPUpdaterAPI.Services.AuthorizationService
{
    internal class AuthorizationService : IAuthorizationService
    {
        private IConfiguration _configuration;
        private readonly string _key = "b14ca5898a4e4133bbce2ea2315a1916";

        public AuthorizationService(IConfiguration configuration) 
        {
            _configuration = configuration;
        }


        private string DecryptTicket(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            string decryptString = "";

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            decryptString = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            return decryptString.Split('|')[0];
        }

        private async Task<bool> CheckUserRoleInDb(string ticketLogin)
        {
            bool isSupport = false;
            string sql = $@"
                            select [IsAdmin] // field in your DB that confirms user has admin rights
                            from [User] // a table of users in your DB
                            where UserLogin = '{ticketLogin}';
                        ";

            using (var connection = new SqlConnection(_configuration["your connection string"]))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        isSupport = Convert.ToBoolean(reader.GetValue(0));
                    }
                }

                await reader.CloseAsync();
            }

            return isSupport;
        }

        async Task<bool> IAuthorizationService.CheckIfUserIsAdmin(string? ticket)
        {
            if (ticket == null)
            {
                throw new NoAuthenticationException();
            }

            string ticketLogin = DecryptTicket(ticket);

            return await CheckUserRoleInDb(ticketLogin);
            
        }
    }
}
